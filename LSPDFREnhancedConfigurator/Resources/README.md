# Resources Directory

This directory contains all image and icon resources for the LSPDFR Enhanced Configurator application.

## Directory Structure

```
Resources/
├── Icons/          # Application icons and UI symbols
│   ├── warning.png     # Warning/exclamation icon (⚠️)
│   ├── error.png       # Error icon (❌)
│   ├── success.png     # Success/checkmark icon (✓)
│   ├── info.png        # Information icon (ℹ️)
│   └── app-icon.ico    # Main application icon (for window/taskbar)
│
└── Images/         # Larger images (logos, splash screens, etc.)
    ├── logo.png        # Application logo
    ├── logo-light.png  # Light theme variant (if needed)
    └── logo-dark.png   # Dark theme variant (if needed)
```

## Icon Specifications

### UI Icons (warning, error, success, info)
- **Format:** PNG with transparency
- **Recommended sizes:** 16x16, 24x24, 32x32 pixels
- **Naming convention:** `icon-name-{size}.png` (e.g., `warning-16.png`, `warning-24.png`)
- **Usage:** Inline with text in lists, next to invalid entries, in validation messages

### Application Icon (app-icon.ico)
- **Format:** ICO file with multiple resolutions
- **Recommended sizes:** 16x16, 32x32, 48x48, 64x64, 128x128, 256x256
- **Usage:** Window icon, taskbar icon, file associations

## Image Specifications

### Logo
- **Format:** PNG with transparency (for flexible background usage)
- **Recommended size:** 512x512 pixels (high resolution for scaling)
- **Variants:** Consider light and dark theme versions
- **Usage:**
  - Welcome screen
  - About dialog
  - Loading screen
  - Documentation

## Usage in Code

### Loading Icons
```csharp
// Example: Load a warning icon
var warningIcon = new Bitmap("Resources/Icons/warning-16.png");
var pictureBox = new PictureBox { Image = warningIcon };
```

### Loading Images
```csharp
// Example: Load application logo
var logo = new Bitmap("Resources/Images/logo.png");
var logoPictureBox = new PictureBox { Image = logo, SizeMode = PictureBoxSizeMode.Zoom };
```

### Setting Application Icon
```csharp
// In MainForm constructor or designer
this.Icon = new Icon("Resources/Icons/app-icon.ico");
```

## Adding Resources to Project

To ensure resources are included in the build:

1. Add the resource files to the project
2. Set the **Build Action** to `Content`
3. Set **Copy to Output Directory** to `Copy if newer`

Or add to `.csproj` file:
```xml
<ItemGroup>
  <Content Include="Resources\Icons\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Include="Resources\Images\**\*.*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

## Design Guidelines

### Color Scheme
- **Warning:** Orange/Amber (#FFA500, #FF8C00)
- **Error:** Red (#DC3545, #C82333)
- **Success:** Green (#28A745, #218838)
- **Info:** Blue (#17A2B8, #138496)

### Style
- Use consistent icon style (flat, outlined, or filled)
- Maintain visual consistency with Windows Forms themes
- Ensure icons are readable at small sizes (16x16)
- Use transparency for flexible background integration

## Recommended Icon Sources

- **Free Icons:**
  - [Flaticon](https://www.flaticon.com/)
  - [Font Awesome](https://fontawesome.com/) (convert to PNG)
  - [Material Icons](https://fonts.google.com/icons)
  - [Heroicons](https://heroicons.com/)

- **Icon Tools:**
  - [IcoMoon](https://icomoon.io/) - Create custom icon fonts and PNGs
  - [RealFaviconGenerator](https://realfavicongenerator.net/) - Generate ICO files
  - [ImageMagick](https://imagemagick.org/) - Batch convert/resize

## Current Status

- [ ] Application logo designed
- [ ] Warning icon added
- [ ] Error icon added
- [ ] Success icon added
- [ ] Info icon added
- [ ] Application ICO file created
- [ ] Resources integrated into codebase
- [ ] ASCII symbols (⚠️) replaced with icon images

## Future Enhancements

- Theme-aware icons (light/dark mode variants)
- Animated loading spinner
- Custom button icons
- Tab icons
- Toolbar icons (if toolbar is added)
