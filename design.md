# Guía de Diseño y UI (Design System)

Este documento define las directrices visuales, tipográficas y la paleta de colores para la capa de presentación de la aplicación.

---

## Stack Tecnológico de UI

- **Framework**: **WPF (.NET)**.
- **Librería de Componentes / UI Kit**: En evaluación (definiendo entre opciones como **WPF-UI** o **Material Design in XAML Toolkit**).

---

## 1. Tipografías

- **Títulos y Encabezados**: `Noto Serif` (usada en h1-h6, títulos de tarjetas y encabezados de sidebar).
- **Cuerpo y UI**: `Be Vietnam Pro` (pesos: Regular 400, Medium 500, Bold 700 e Italic).

> [!NOTE]
> **En WPF**: Se pueden incrustar los archivos `.ttf` de `public/fonts/` (o `Assets/Fonts/`) como recursos en el proyecto C# y referenciarlos en XAML como `./Fonts/#Noto Serif` y `./Fonts/#Be Vietnam Pro`.

---

## 2. Paleta de Colores (Tokens)

| Rol | Hex | Uso en la App | Equivalente XAML sugerido |
| :--- | :--- | :--- | :--- |
| **Primario (Marrón)** | `#622B16` | Navbar, botones principales, títulos, bordes | `PrimaryBrush` |
| **Acento (Terracota)** | `#9A4600` | Links destacados (`.text-maie`), foco de inputs | `AccentBrush` |
| **Secundario (Canela)** | `#BE8E75` | Bordes de inputs, links secundarios | `SecondaryBrush` |
| **Background (Beige claro)** | `#FCF9F4` | Fondo general de la ventana y vistas | `BackgroundBrush` |
| **Surface Alt (Crema)** | `#F6F3EE` | Fondo de catálogo, secciones alternadas | `SurfaceAltBrush` |
| **Hover Primario** | `#4A2011` | Estado `:hover` y `:pressed` de botones | `PrimaryHoverBrush` |
| **Texto de Cuerpo** | `#555555` | Párrafos y descripciones estándar | `TextBodyBrush` |
| **Surface (Blanco)** | `#FFFFFF` | Fondo de tarjetas y paneles de contenido | `SurfaceBrush` |
