/* =============================================================================
   DATASET DE PRUEBA PARA REPORTES Y ESTADISTICAS
   Base: ProyectoCRUD (SQL Server Express, instancia localhost\SQLEXPRESS)

   COMO USARLO
     1) Abrir la app al menos una vez (el DatabaseSeeder crea roles, sucursales,
        usuarios base, categorias e insumos iniciales).
     2) Desde la raiz del repo:
          sqlcmd -S "localhost\SQLEXPRESS" -E -C -I -f 65001 -i Datos\Scripts\dataset_pruebas.sql
        (o abrirlo en SSMS y ejecutarlo; SSMS ya usa QUOTED_IDENTIFIER ON)
        El -I es obligatorio: el indice filtrado de ventas PENDIENTE exige
        QUOTED_IDENTIFIER ON y sqlcmd lo trae apagado por defecto.

   QUE HACE (es seguro volver a correrlo)
     - Completa el catalogo si falta: insumos, productos y recetas (solo agrega
       los que no existen por nombre; nunca modifica ni borra los existentes).
     - Asigna costo de produccion (45%-70% del precio) a los productos que no
       tengan costo, para que los reportes de ganancia tengan datos.
     - Completa StocksSucursal para los pares producto/sucursal que no tengan fila.
     - Agrega cocineros extra (solo los que falten por email) para que los reportes
       por cocinero no queden con una sola fila.
     - Agrega clientes de prueba variados (solo los que falten por DNI).
       dataset_ventas.sql los necesita: sin clientes no genera ventas.
     - Genera producciones de los ultimos @Dias dias, con fechas, horas, cocineros,
       sucursales y cantidades variadas (mas actividad los sabados, poca los
       domingos, productos mas y menos populares, tortas en cantidades chicas).
     - Es DETERMINISTICO: siempre genera lo mismo para el mismo dia.
     - Las producciones generadas se marcan con UpdatedAt = 2000-01-01. Al volver
       a correr el script se borran solo esas y se regeneran (las producciones
       reales cargadas desde la app NO se tocan).

   LIMPIAR SOLO LO GENERADO
        DELETE FROM Producciones WHERE UpdatedAt = '2000-01-01';

   NOTAS
     - Los cocineros extra reutilizan el hash del cocinero del seeder, asi que
       inician sesion con la misma contrasena (cocina123).
     - No descuenta stock de insumos ni acumula StocksSucursal: son datos para
       probar reportes, no una simulacion contable.
     - Las ventas se generan aparte con Datos\Scripts\dataset_ventas.sql.
     - Los casos especiales (inactivos, borrados logicos, stock bajo) se cargan
       a mano desde la aplicacion; este script no los inventa.
   ============================================================================= */

USE ProyectoCRUD;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Dias  INT       = 90;                        -- cuantos dias hacia atras
DECLARE @Marca DATETIME2 = '2000-01-01T00:00:00';     -- marca de "dato de prueba"
DECLARE @Ahora DATETIME2 = SYSDATETIME();

-- Precondicion: la app tiene que haber corrido el seeder al menos una vez.
IF NOT EXISTS (SELECT 1 FROM Sucursales WHERE DeletedAt IS NULL)
   OR NOT EXISTS (SELECT 1 FROM Usuarios WHERE DeletedAt IS NULL)
   OR NOT EXISTS (SELECT 1 FROM Categorias)
BEGIN
    RAISERROR('Faltan datos base. Abri la app una vez (corre el seeder) y volve a ejecutar este script.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;

-- 1) INSUMOS (solo los que falten, por nombre) --------------------------------
DECLARE @Insumos TABLE (Nombre NVARCHAR(200), Unidad NVARCHAR(20), Stock DECIMAL(18,3), StockMinimo DECIMAL(18,3));
INSERT INTO @Insumos VALUES
    (N'Harina 0000',              N'kg', 200, 15),
    (N'Dulce de Leche Repostero', N'kg', 150, 12),
    (N'Chocolate Semiamargo',     N'kg', 120, 10),
    (N'Azucar',                   N'kg', 100, 10),
    (N'Manteca',                  N'kg',  60,  8),
    (N'Huevos',                   N'u',  600, 60),
    (N'Maicena',                  N'kg',  50,  5),
    (N'Coco Rallado',             N'kg',  30,  4),
    (N'Leche Entera',             N'l',   80, 10),
    (N'Esencia de Vainilla',      N'ml', 500, 50),
    (N'Sal Fina',                 N'g', 5000, 500);

INSERT INTO Insumos (Nombre, Unidad, Stock, StockMinimo, CreatedAt)
SELECT i.Nombre, i.Unidad, i.Stock, i.StockMinimo, @Ahora
FROM @Insumos i
WHERE NOT EXISTS (SELECT 1 FROM Insumos x WHERE x.Nombre = i.Nombre);

-- 2) PRODUCTOS (solo los que falten, por nombre) ------------------------------
DECLARE @Productos TABLE (Nombre NVARCHAR(200), Precio INT, Categoria NVARCHAR(100));
INSERT INTO @Productos VALUES
    (N'Alfajor de Chocolate',       1200, N'Alfajores'),
    (N'Alfajor de Dulce de Leche',  1100, N'Alfajores'),
    (N'Alfajor Triple',             1500, N'Alfajores'),
    (N'Alfajor de Maicena',         1000, N'Alfajores'),
    (N'Conito de Dulce de Leche',    900, N'Conitos'),
    (N'Conito Banado',              1000, N'Conitos'),
    (N'Tableta Semiamarga',         2500, N'Tabletas'),
    (N'Torta Chocotorta',           8500, N'Tortas'),
    (N'Torta Rogel',                9200, N'Tortas'),
    (N'Caja Surtida x12',          14000, N'Especiales'),
    (N'Caja Surtida x6',            7500, N'Especiales');

INSERT INTO Productos (Nombre, Precio, RutaImagen, CategoriaId, CreatedAt)
SELECT p.Nombre, p.Precio, NULL, c.Id, @Ahora
FROM @Productos p
JOIN Categorias c ON c.Nombre = p.Categoria
WHERE NOT EXISTS (SELECT 1 FROM Productos x WHERE x.Nombre = p.Nombre);

-- 2b) COSTO DE PRODUCCION (solo productos sin costo) ---------------------------
-- Costo deterministico entre 45% y 70% del precio, para que los reportes de
-- ganancia tengan datos. Los productos con costo ya cargado NO se tocan.
UPDATE Productos
SET Costo = ROUND(Precio * (45 + (Id * 7) % 26) / 100.0, 0)
WHERE DeletedAt IS NULL AND Costo = 0;

-- 3) RECETAS (solo para productos que todavia no tengan ninguna) --------------
DECLARE @Recetas TABLE (Producto NVARCHAR(200), Insumo NVARCHAR(200), Cantidad DECIMAL(18,3));
INSERT INTO @Recetas VALUES
    (N'Alfajor de Chocolate',      N'Harina 0000', 0.030), (N'Alfajor de Chocolate',      N'Chocolate Semiamargo', 0.020), (N'Alfajor de Chocolate',      N'Dulce de Leche Repostero', 0.015),
    (N'Alfajor de Dulce de Leche', N'Harina 0000', 0.030), (N'Alfajor de Dulce de Leche', N'Dulce de Leche Repostero', 0.035), (N'Alfajor de Dulce de Leche', N'Azucar', 0.010),
    (N'Alfajor Triple',            N'Harina 0000', 0.040), (N'Alfajor Triple',            N'Chocolate Semiamargo', 0.030), (N'Alfajor Triple',            N'Dulce de Leche Repostero', 0.030),
    (N'Alfajor de Maicena',        N'Maicena',     0.040), (N'Alfajor de Maicena',        N'Dulce de Leche Repostero', 0.030), (N'Alfajor de Maicena',        N'Coco Rallado', 0.008),
    (N'Conito de Dulce de Leche',  N'Harina 0000', 0.020), (N'Conito de Dulce de Leche',  N'Dulce de Leche Repostero', 0.040),
    (N'Conito Banado',             N'Harina 0000', 0.020), (N'Conito Banado',             N'Dulce de Leche Repostero', 0.030), (N'Conito Banado',             N'Chocolate Semiamargo', 0.025),
    (N'Tableta Semiamarga',        N'Chocolate Semiamargo', 0.100), (N'Tableta Semiamarga', N'Manteca', 0.010), (N'Tableta Semiamarga', N'Azucar', 0.020),
    (N'Torta Chocotorta',          N'Harina 0000', 0.300), (N'Torta Chocotorta',          N'Dulce de Leche Repostero', 0.250), (N'Torta Chocotorta',          N'Huevos', 4),
    (N'Torta Rogel',               N'Harina 0000', 0.350), (N'Torta Rogel',               N'Dulce de Leche Repostero', 0.400), (N'Torta Rogel',               N'Huevos', 6),
    (N'Caja Surtida x12',          N'Chocolate Semiamargo', 0.150), (N'Caja Surtida x12', N'Dulce de Leche Repostero', 0.200), (N'Caja Surtida x12', N'Harina 0000', 0.250),
    (N'Caja Surtida x6',           N'Chocolate Semiamargo', 0.080), (N'Caja Surtida x6',  N'Dulce de Leche Repostero', 0.100), (N'Caja Surtida x6',  N'Harina 0000', 0.120);

INSERT INTO ProductosInsumos (InsumoId, ProductoId, CantidadNecesaria)
SELECT i.Id, p.Id, r.Cantidad
FROM @Recetas r
JOIN Productos p ON p.Nombre = r.Producto AND p.DeletedAt IS NULL
JOIN Insumos   i ON i.Nombre = r.Insumo   AND i.DeletedAt IS NULL
WHERE NOT EXISTS (SELECT 1 FROM ProductosInsumos pi WHERE pi.ProductoId = p.Id);

-- 4) STOCK POR SUCURSAL (solo los pares producto/sucursal sin fila) ------------
INSERT INTO StocksSucursal (Cantidad, CreatedAt, UpdatedAt, ProductoId, SucursalId)
SELECT (CHECKSUM(p.Id, s.Id, 99) & 0x7FFFFFFF) % 121, @Ahora, NULL, p.Id, s.Id
FROM Productos p
CROSS JOIN Sucursales s
WHERE p.DeletedAt IS NULL AND s.DeletedAt IS NULL
  AND NOT EXISTS (SELECT 1 FROM StocksSucursal ss WHERE ss.ProductoId = p.Id AND ss.SucursalId = s.Id);

-- 5) COCINEROS ADICIONALES (para que los reportes por cocinero tengan variedad) --
-- Reutilizan el hash del cocinero del seeder para poder iniciar sesion con cocina123.
DECLARE @PassDemo NVARCHAR(200) = (
    SELECT TOP 1 Password FROM Usuarios WHERE Email = N'cocinero@test.com'
);
IF @PassDemo IS NULL
    SET @PassDemo = (SELECT TOP 1 Password FROM Usuarios WHERE Email = N'admin@test.com');
IF @PassDemo IS NULL
    SET @PassDemo = (SELECT TOP 1 Password FROM Usuarios ORDER BY Id);

INSERT INTO Usuarios (Nombre, Apellido, Dni, Email, Password, FechaNacimiento, Direccion, RolId, SucursalId, CreatedAt)
SELECT u.Nombre, u.Apellido, u.Dni, u.Email, @PassDemo, u.FechaNacimiento, u.Direccion, r.Id, s.Id, @Ahora
FROM (VALUES
    (N'Santiago', N'Quiroga', N'33456789', N'santiago.quiroga@test.com', CAST('1993-03-18' AS DATE), N'Rivadavia 845',  N'Fábrica'),
    (N'Brenda',   N'Ojeda',   N'36789451', N'brenda.ojeda@test.com',     CAST('1997-08-05' AS DATE), N'Bolivar 1120',   N'Casa Central')
) AS u(Nombre, Apellido, Dni, Email, FechaNacimiento, Direccion, Sucursal)
JOIN Roles      r ON r.Nombre = N'Cocinero' AND r.DeletedAt IS NULL
JOIN Sucursales s ON s.Nombre = u.Sucursal  AND s.DeletedAt IS NULL
WHERE NOT EXISTS (SELECT 1 FROM Usuarios x WHERE x.Email = u.Email);

-- 6) CLIENTES (dataset_ventas.sql los necesita para generar ventas) -------------
DECLARE @Clientes TABLE (
    Orden INT, Nombre NVARCHAR(100), Apellido NVARCHAR(100),
    Dni NVARCHAR(20), Email NVARCHAR(200), FechaNacimiento DATE
);
INSERT INTO @Clientes VALUES
    ( 1, N'Sofia',     N'Gimenez',   N'38456123', N'sofia.gimenez@mail.com',   '1994-03-12'),
    ( 2, N'Martin',    N'Aguirre',   N'30124567', N'martin.aguirre@mail.com',  '1983-07-25'),
    ( 3, N'Lucia',     N'Fernandez', N'41235879', N'lucia.fernandez@mail.com', '1999-11-02'),
    ( 4, N'Joaquin',   N'Pereyra',   N'28987456', N'joaquin.pereyra@mail.com', '1977-01-19'),
    ( 5, N'Valentina', N'Sosa',      N'43561278', N'valentina.sosa@mail.com',  '2002-06-08'),
    ( 6, N'Nicolas',   N'Benitez',   N'35678412', N'nicolas.benitez@mail.com', '1990-09-30'),
    ( 7, N'Camila',    N'Rios',      N'39874561', N'camila.rios@mail.com',     '1996-04-17'),
    ( 8, N'Facundo',   N'Molina',    N'26789431', N'facundo.molina@mail.com',  '1972-12-05'),
    ( 9, N'Martina',   N'Cabrera',   N'44781236', N'martina.cabrera@mail.com', '2004-02-23'),
    (10, N'Agustin',   N'Silva',     N'32147896', N'agustin.silva@mail.com',   '1986-08-14'),
    (11, N'Julieta',   N'Acosta',    N'37512348', N'julieta.acosta@mail.com',  '1992-10-29'),
    (12, N'Tomas',     N'Herrera',   N'25369781', N'tomas.herrera@mail.com',   '1968-05-07');

-- Registradores posibles: vendedores y admin (si no hubiera, cualquier usuario activo).
DECLARE @Registradores TABLE (Orden INT IDENTITY(1,1), Id INT);
INSERT INTO @Registradores (Id)
SELECT u.Id
FROM Usuarios u JOIN Roles r ON r.Id = u.RolId
WHERE u.DeletedAt IS NULL AND r.DeletedAt IS NULL
  AND r.Nombre IN (N'Vendedor', N'Admin')
ORDER BY u.Id;

IF NOT EXISTS (SELECT 1 FROM @Registradores)
    INSERT INTO @Registradores (Id)
    SELECT u.Id FROM Usuarios u WHERE u.DeletedAt IS NULL ORDER BY u.Id;

DECLARE @NReg INT = (SELECT COUNT(*) FROM @Registradores);

INSERT INTO Clientes (Nombre, Apellido, Dni, Email, FechaNacimiento, UsuarioId, CreatedAt)
SELECT c.Nombre, c.Apellido, c.Dni, c.Email, c.FechaNacimiento, reg.Id, @Ahora
FROM @Clientes c
JOIN @Registradores reg ON reg.Orden = (c.Orden - 1) % @NReg + 1
WHERE NOT EXISTS (SELECT 1 FROM Clientes x WHERE x.Dni = c.Dni);

-- 7) PRODUCCIONES --------------------------------------------------------------
-- Se borran solo las generadas por una corrida anterior (las identifica la marca).
DELETE FROM Producciones WHERE UpdatedAt = @Marca;

-- Cocineros disponibles (si no hubiera, se usan los admin).
DECLARE @Cocineros TABLE (Orden INT IDENTITY(1,1), Id INT);
INSERT INTO @Cocineros (Id)
SELECT u.Id
FROM Usuarios u JOIN Roles r ON r.Id = u.RolId
WHERE r.Nombre = N'Cocinero' AND u.DeletedAt IS NULL AND r.DeletedAt IS NULL
ORDER BY u.Id;

IF NOT EXISTS (SELECT 1 FROM @Cocineros)
    INSERT INTO @Cocineros (Id)
    SELECT u.Id
    FROM Usuarios u JOIN Roles r ON r.Id = u.RolId
    WHERE r.Nombre = N'Admin' AND u.DeletedAt IS NULL
    ORDER BY u.Id;

DECLARE @NCocineros INT = (SELECT COUNT(*) FROM @Cocineros);

;WITH Dias AS (
    -- 0 = hoy, 1 = ayer, ...
    SELECT TOP (@Dias) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS Desfase
    FROM sys.all_objects
),
Base AS (
    SELECT
        CAST(DATEADD(DAY, -d.Desfase, @Ahora) AS DATE) AS Dia,
        p.Id   AS ProductoId,
        cat.Nombre AS Categoria,
        s.Id   AS SucursalId,
        -- Numero pseudo-aleatorio estable por (dia, producto, sucursal)
        CHECKSUM(d.Desfase, p.Id, s.Id) & 0x7FFFFFFF AS H
    FROM Dias d
    CROSS JOIN Productos p
    JOIN Categorias cat ON cat.Id = p.CategoriaId
    CROSS JOIN Sucursales s
    WHERE p.DeletedAt IS NULL AND s.DeletedAt IS NULL
      AND EXISTS (SELECT 1 FROM ProductosInsumos pi WHERE pi.ProductoId = p.Id)
),
Calc AS (
    SELECT b.*,
           DATEDIFF(DAY, 0, b.Dia) % 7 AS Dow    -- 0 = lunes ... 5 = sabado, 6 = domingo
    FROM Base b
),
Final AS (
    SELECT c.*,
           -- probabilidad (%) de que ese producto se elabore ese dia en esa sucursal
           CASE WHEN c.Dow = 6 THEN 15 WHEN c.Dow = 5 THEN 60 ELSE 45 END
             + ((c.ProductoId * 7) % 5 - 2) * 8                -- productos mas / menos populares
             + CASE WHEN c.SucursalId = 2 THEN 10 ELSE 0 END    -- una sucursal produce mas
             AS Prob,
           -- cantidad base segun categoria
           CASE c.Categoria
                WHEN N'Tortas'     THEN 2 + (c.H / 100) % 7
                WHEN N'Especiales' THEN 3 + (c.H / 100) % 12
                ELSE                    10 + (c.H / 100) % 46
           END AS CantBase
    FROM Calc c
)
INSERT INTO Producciones (Cantidad, CreatedAt, UpdatedAt, DeletedAt, UsuarioId, ProductoId, SucursalId)
SELECT
    f.CantBase * CASE WHEN f.Dow = 5 THEN 3 ELSE 2 END / 2,                                 -- sabados +50%
    DATEADD(MINUTE, 360 + (f.H / 1000) % 720, CAST(f.Dia AS DATETIME2)),                    -- entre 06:00 y 17:59
    @Marca,
    NULL,
    co.Id,
    f.ProductoId,
    f.SucursalId
FROM Final f
JOIN @Cocineros co ON co.Orden = (f.H / 3) % @NCocineros + 1
WHERE (f.H % 100) < f.Prob
  AND DATEADD(MINUTE, 360 + (f.H / 1000) % 720, CAST(f.Dia AS DATETIME2)) <= @Ahora;      -- nada en el futuro

COMMIT TRANSACTION;

-- Resumen ----------------------------------------------------------------------
SELECT
    (SELECT COUNT(*) FROM Productos WHERE DeletedAt IS NULL)                  AS ProductosActivos,
    (SELECT COUNT(*) FROM Productos WHERE DeletedAt IS NULL AND Costo = 0)    AS ProductosSinCosto,
    (SELECT COUNT(DISTINCT ProductoId) FROM ProductosInsumos)                 AS ProductosConReceta,
    (SELECT COUNT(*) FROM StocksSucursal)                                     AS FilasStock,
    (SELECT COUNT(*) FROM Clientes WHERE DeletedAt IS NULL)                   AS ClientesActivos,
    (SELECT COUNT(*) FROM Usuarios u JOIN Roles r ON r.Id = u.RolId
      WHERE u.DeletedAt IS NULL AND r.Nombre = N'Cocinero')                   AS Cocineros,
    (SELECT COUNT(*) FROM Producciones WHERE UpdatedAt = @Marca)              AS ProduccionesGeneradas,
    (SELECT COUNT(*) FROM Producciones WHERE UpdatedAt IS NULL)               AS ProduccionesReales,
    (SELECT MIN(CAST(CreatedAt AS DATE)) FROM Producciones WHERE UpdatedAt = @Marca) AS DesdeFecha,
    (SELECT MAX(CAST(CreatedAt AS DATE)) FROM Producciones WHERE UpdatedAt = @Marca) AS HastaFecha,
    (SELECT COUNT(DISTINCT CAST(CreatedAt AS DATE)) FROM Producciones WHERE UpdatedAt = @Marca) AS DiasConDatos;
