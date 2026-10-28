/* =============================================================================
   DATASET DE VENTAS PARA PROBAR REPORTES Y ESTADISTICAS
   Base: ProyectoCRUD (SQL Server Express, instancia localhost\SQLEXPRESS)

   REQUISITOS
     1) La base tiene que tener las migraciones aplicadas, incluida AgregarCostos
        (columnas Productos.Costo y VentasDetalle.CostoUnit). Se aplican abriendo la app una vez.
     2) Tiene que haber al menos: 1 usuario con rol Vendedor, 1 cliente activo y
        1 producto activo con precio. (Los crea el seeder / datos_prueba.sql.)

   USO (desde la raiz del repo)
        sqlcmd -S "localhost\SQLEXPRESS" -E -C -d ProyectoCRUD -f 65001 -b -i Datos\Scripts\dataset_ventas.sql

   QUE HACE (es seguro volver a correrlo)
     - Genera ventas de los ultimos @Dias dias en las sucursales que venden
       (todas las activas menos Casa Central y Fabrica).
     - A los productos activos que tengan Costo = 0 les asigna un costo de produccion
       de entre el 45% y el 70% del precio (determinista por producto). Los productos
       con un costo ya cargado NO se tocan. Cada renglon de venta guarda ese costo en
       CostoUnit, para poder calcular la ganancia.
     - Si @CrearSucursalesExtra = 1 y faltan, crea "Sucursal Norte" y "Sucursal Sur"
       (con filas de stock) para poder probar reportes por sucursal.
     - Mas ventas viernes y sabado, pocas los domingos, tendencia levemente creciente,
       productos baratos mas vendidos que los caros, ~8% de ventas CANCELADAS,
       metodos de pago repartidos (efectivo / tarjeta / transferencia).
     - Es DETERMINISTICO: el mismo dia siempre genera lo mismo.
     - Solo genera ventas CONFIRMADAS y CANCELADAS (ninguna PENDIENTE).
     - Las ventas generadas se marcan con UpdatedAt = 2000-01-01. Al volver a correr
       el script se borran solo esas y se regeneran. Las ventas reales hechas desde
       la app NO se tocan.

   LIMPIAR SOLO LO GENERADO
        DELETE d FROM VentasDetalle d JOIN VentasCabecera c ON c.Id = d.VentaCabeceraId WHERE c.UpdatedAt = '2000-01-01';
        DELETE FROM VentasCabecera WHERE UpdatedAt = '2000-01-01';

   NOTAS
     - No modifica el stock por las ventas generadas: son datos para probar reportes,
       no una simulacion contable. (Si crea sucursales extra, si les crea filas de stock.)
     - Cumple las restricciones CHECK de la base: las confirmadas y canceladas tienen
       cliente y metodo de pago; cantidades y precios son positivos.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Dias                 INT       = 90;                       -- cuantos dias hacia atras
DECLARE @CrearSucursalesExtra BIT       = 1;                        -- 1 = crear Norte y Sur si faltan
DECLARE @Marca                DATETIME2 = '2000-01-01T00:00:00';    -- marca de "dato de prueba"
DECLARE @Ahora                DATETIME2 = SYSDATETIME();

IF OBJECT_ID(N'VentasCabecera', N'U') IS NULL OR OBJECT_ID(N'VentasDetalle', N'U') IS NULL
    THROW 50010, 'No existen las tablas de ventas. Abri la app una vez para aplicar las migraciones.', 1;

IF COL_LENGTH(N'Productos', N'Costo') IS NULL OR COL_LENGTH(N'VentasDetalle', N'CostoUnit') IS NULL
    THROW 50012, 'Faltan las columnas de costo (migracion AgregarCostos). Abri la app una vez para aplicarla.', 1;

BEGIN TRANSACTION;

-- 0) Limpieza de lo generado en una corrida anterior (las identifica la marca) ----
DELETE d
FROM VentasDetalle d
JOIN VentasCabecera c ON c.Id = d.VentaCabeceraId
WHERE c.UpdatedAt = @Marca;

DELETE FROM VentasCabecera WHERE UpdatedAt = @Marca;

-- 1) Sucursales extra (opcional) ------------------------------------------------
IF @CrearSucursalesExtra = 1
BEGIN
    INSERT INTO Sucursales (Nombre, Ubicacion, Estado, CreatedAt)
    SELECT v.Nombre, v.Ubicacion, N'ACTIVA', @Ahora
    FROM (VALUES (N'Sucursal Norte', N'Av. Libertad 450'),
                 (N'Sucursal Sur',   N'Belgrano 2200')) AS v(Nombre, Ubicacion)
    WHERE NOT EXISTS (SELECT 1 FROM Sucursales s WHERE s.Nombre = v.Nombre);
END

-- 2) Stock de producto/sucursal para los pares que no tengan fila ----------------
INSERT INTO StocksSucursal (Cantidad, CreatedAt, UpdatedAt, ProductoId, SucursalId)
SELECT (CHECKSUM(p.Id, s.Id, 99) & 0x7FFFFFFF) % 121, @Ahora, NULL, p.Id, s.Id
FROM Productos p
CROSS JOIN Sucursales s
WHERE p.DeletedAt IS NULL AND s.DeletedAt IS NULL
  AND NOT EXISTS (SELECT 1 FROM StocksSucursal ss WHERE ss.ProductoId = p.Id AND ss.SucursalId = s.Id);

-- 2b) Costos de produccion: solo para los productos activos que todavia no tienen costo ----
UPDATE Productos
SET Costo = ROUND(Precio * (45 + (Id * 7) % 26) / 100.0, 0)
WHERE DeletedAt IS NULL AND Costo = 0;

-- 3) Tablas auxiliares: quienes participan ---------------------------------------
-- Sucursales que venden (Casa Central y Fabrica son administracion / produccion).
SELECT ROW_NUMBER() OVER (ORDER BY s.Id) AS Orden, s.Id AS SucursalId
INTO #Suc
FROM Sucursales s
WHERE s.DeletedAt IS NULL
  AND s.Nombre <> N'Casa Central'
  AND s.Nombre NOT LIKE N'F%brica';

SELECT ROW_NUMBER() OVER (ORDER BY p.Id) AS Orden, p.Id AS ProductoId, p.Precio, p.Costo
INTO #Prod
FROM Productos p
WHERE p.DeletedAt IS NULL AND p.Precio > 0;

SELECT ROW_NUMBER() OVER (ORDER BY c.Id) AS Orden, c.Id AS ClienteId
INTO #Cli
FROM Clientes c
WHERE c.DeletedAt IS NULL;

SELECT ROW_NUMBER() OVER (ORDER BY u.Id) AS Orden, u.Id AS UsuarioId
INTO #Ven
FROM Usuarios u
JOIN Roles r ON r.Id = u.RolId
WHERE r.Nombre = N'Vendedor' AND u.DeletedAt IS NULL AND r.DeletedAt IS NULL;

DECLARE @NSuc  INT = (SELECT COUNT(*) FROM #Suc);
DECLARE @NProd INT = (SELECT COUNT(*) FROM #Prod);
DECLARE @NCli  INT = (SELECT COUNT(*) FROM #Cli);
DECLARE @NVen  INT = (SELECT COUNT(*) FROM #Ven);

IF @NSuc = 0 OR @NProd = 0 OR @NCli = 0 OR @NVen = 0
    THROW 50011, 'Faltan datos base: hace falta al menos 1 sucursal que venda, 1 producto con precio, 1 cliente y 1 vendedor.', 1;

-- 4) Tickets: cuantas ventas hay por dia y sucursal ------------------------------
;WITH Dias AS (
    -- 0 = hoy, 1 = ayer, ...
    SELECT TOP (@Dias) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS Desfase
    FROM sys.all_objects
),
Ks AS (
    SELECT TOP (30) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS K
    FROM sys.all_objects
),
Base AS (
    SELECT d.Desfase,
           CAST(DATEADD(DAY, -d.Desfase, @Ahora) AS DATE) AS Dia,
           sc.SucursalId,
           sc.Orden AS SucOrden,
           CHECKSUM(d.Desfase, sc.SucursalId, 7) & 0x7FFFFFFF AS HDia
    FROM Dias d
    CROSS JOIN #Suc sc
),
Calc AS (
    SELECT b.*, DATEDIFF(DAY, 0, b.Dia) % 7 AS Dow     -- 0 = lunes ... 4 = viernes, 5 = sabado, 6 = domingo
    FROM Base b
),
Cant AS (
    SELECT c.*,
           CASE WHEN c.Dow = 6 THEN 4
                WHEN c.Dow = 5 THEN 12
                WHEN c.Dow = 4 THEN 8
                ELSE 5 END
           + (c.HDia % 5) - 2                                          -- variacion diaria
           + CASE c.SucOrden WHEN 1 THEN 3 WHEN 3 THEN -1 ELSE 0 END    -- unas sucursales venden mas
           + (@Dias - c.Desfase) / 30                                   -- tendencia creciente
           AS NTickets
    FROM Calc c
)
SELECT ROW_NUMBER() OVER (ORDER BY n.Desfase DESC, n.SucursalId, k.K) AS TicketId,
       n.SucursalId,
       -- entre las 08:00 y las 20:59, con k segundos para que dos ventas no tengan la misma hora exacta
       DATEADD(SECOND, k.K, DATEADD(MINUTE, 480 + (CHECKSUM(n.Desfase, n.SucursalId, k.K) & 0x7FFFFFFF) / 10 % 780,
               CAST(n.Dia AS DATETIME2))) AS Fecha,
       CHECKSUM(n.Desfase, n.SucursalId, k.K) & 0x7FFFFFFF AS H
INTO #Tickets
FROM Cant n
JOIN Ks k ON k.K <= n.NTickets;

-- nada en el futuro
DELETE FROM #Tickets WHERE Fecha > @Ahora;

-- 5) Cabeceras: MERGE con ON 1=0 para poder devolver el Id nuevo de cada ticket ----
CREATE TABLE #Mapa (TicketId BIGINT NOT NULL, VentaId INT NOT NULL);

MERGE VentasCabecera AS destino
USING (
    SELECT t.TicketId, t.SucursalId, t.Fecha, t.H,
           v.UsuarioId, c.ClienteId
    FROM #Tickets t
    JOIN #Ven v ON v.Orden = (t.H / 7)  % @NVen + 1
    JOIN #Cli c ON c.Orden = (t.H / 13) % @NCli + 1
) AS origen
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (SucursalId, UsuarioId, ClienteId, MetodoPago, Estado, Total, CreatedAt, UpdatedAt)
    VALUES (
        origen.SucursalId,
        origen.UsuarioId,
        origen.ClienteId,
        CASE WHEN (origen.H / 19) % 100 < 45 THEN N'EFECTIVO'
             WHEN (origen.H / 19) % 100 < 80 THEN N'TARJETA'
             ELSE N'TRANSFERENCIA' END,
        CASE WHEN (origen.H / 17) % 100 < 8 THEN N'CANCELADA' ELSE N'CONFIRMADA' END,
        0,
        origen.Fecha,
        @Marca)
OUTPUT origen.TicketId, inserted.Id INTO #Mapa (TicketId, VentaId);

-- 6) Detalles: 1 a 4 productos por ticket, sesgados hacia los primeros del catalogo ---
;WITH Js AS (
    SELECT TOP (4) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS J
    FROM sys.all_objects
),
Lineas AS (
    SELECT t.TicketId, t.Fecha,
           CHECKSUM(t.TicketId, j.J, 31) & 0x7FFFFFFF AS H2
    FROM #Tickets t
    JOIN Js j ON j.J <= 1 + (t.H / 23) % 4
),
Elegidas AS (
    SELECT l.TicketId, l.Fecha,
           -- el producto de un valor chico se elige mas seguido (a*b/n favorece los chicos)
           ((l.H2 % @NProd) * ((l.H2 / 101) % @NProd)) / @NProd + 1 AS PIdx,
           1 + (l.H2 / 211) % 6 AS Cant
    FROM Lineas l
)
INSERT INTO VentasDetalle (VentaCabeceraId, ProductoId, Cantidad, PrecioUnit, CostoUnit, CreatedAt)
SELECT m.VentaId, p.ProductoId, SUM(e.Cant), p.Precio, p.Costo, MIN(e.Fecha)
FROM Elegidas e
JOIN #Mapa m ON m.TicketId = e.TicketId
JOIN #Prod p ON p.Orden = e.PIdx
GROUP BY m.VentaId, p.ProductoId, p.Precio, p.Costo;

-- 7) Total de cada cabecera = suma de sus detalles -------------------------------
UPDATE c
SET Total = x.Tot
FROM VentasCabecera c
JOIN (SELECT VentaCabeceraId, SUM(Cantidad * PrecioUnit) AS Tot
      FROM VentasDetalle
      GROUP BY VentaCabeceraId) x ON x.VentaCabeceraId = c.Id
WHERE c.UpdatedAt = @Marca;

COMMIT TRANSACTION;

-- Resumen ------------------------------------------------------------------------
SELECT
    (SELECT COUNT(*) FROM VentasCabecera WHERE UpdatedAt = @Marca)                                  AS VentasGeneradas,
    (SELECT COUNT(*) FROM VentasCabecera WHERE UpdatedAt = @Marca AND Estado = N'CONFIRMADA')       AS Confirmadas,
    (SELECT COUNT(*) FROM VentasCabecera WHERE UpdatedAt = @Marca AND Estado = N'CANCELADA')        AS Canceladas,
    (SELECT COUNT(*) FROM VentasDetalle d JOIN VentasCabecera c ON c.Id = d.VentaCabeceraId
      WHERE c.UpdatedAt = @Marca)                                                                   AS Renglones,
    (SELECT MIN(CAST(CreatedAt AS DATE)) FROM VentasCabecera WHERE UpdatedAt = @Marca)              AS DesdeFecha,
    (SELECT MAX(CAST(CreatedAt AS DATE)) FROM VentasCabecera WHERE UpdatedAt = @Marca)              AS HastaFecha,
    (SELECT COUNT(DISTINCT CAST(CreatedAt AS DATE)) FROM VentasCabecera WHERE UpdatedAt = @Marca)   AS DiasConDatos,
    (SELECT SUM(Total) FROM VentasCabecera WHERE UpdatedAt = @Marca AND Estado = N'CONFIRMADA')     AS IngresosConfirmadas,
    (SELECT COUNT(*) FROM VentasCabecera WHERE UpdatedAt IS NULL OR UpdatedAt <> @Marca)            AS VentasReales;

DROP TABLE #Mapa;
DROP TABLE #Tickets;
DROP TABLE #Suc;
DROP TABLE #Prod;
DROP TABLE #Cli;
DROP TABLE #Ven;
