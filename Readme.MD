# LogicAtlas (Core CLI)

A lightweight, automated Sprite Packer for build pipelines. 
Generates texture atlases and JSON data files with embedded logic.

**✨ Want visuals? Drag-and-drop? Visual Pivot Editing?**
👉 **[Get LogicAtlas PRO on Itch.io](https://gregorigin.itch.io/logicatlas)**  
*(The Pro version includes a GUI, visual anchor editing, and individual sprite settings)*

## 🚀 Features
- **Shelf Packing Algorithm:** Efficiently packs sprites by height.
- **JSON Data Export:** Includes coordinates, dimensions, and pivot logic.
- **Pipeline Ready:** Designed for CI/CD, batch scripts, and automation.
- **Zero Dependencies:** Single .exe, runs on any Windows machine.

## 💻 Usage

bash
LogicAtlas-CLI.exe <InputFolder> [Flags]

## Flags

-o Output filename (default: atlas)

--padding Pixel padding between sprites (default: 2)

--pivot Global pivot setting (Default: C)

## Pivot Modes:

C : Center (0.5, 0.5)

TL: Top-Left (0.0, 0.0)

BC: Bottom-Center (0.5, 1.0) - Great for characters

## Example

To pack a folder of RPG characters with Bottom-Center pivots (for feet alignment): code Bash

LogicAtlas-CLI.exe "./Sprites/Characters" -o Chars --pivot BC
## 📦 Integration (Godot Example)

The tool outputs a standard JSON. Parse it to set up your sprites automatically. code Gdscript
Godot 4.x

var data = JSON.parse_string(json_text) $Sprite2D.region_rect = Rect2(data.x, data.y, data.w, data.h) $Sprite2D.offset = Vector2(data.w * data.pivot_x, data.h * data.pivot_y)

📄 License

MIT License.