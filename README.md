# Unity Spline Object Placer Editor

**Spline Object Placer Editor** is a Unity Editor tool that lets you quickly and intuitively place objects along a spline in your scene. Designed for level designers and technical artists, this script streamlines the process of populating paths, roads, fences, or any other spline-based layout with prefabs—saving you time and giving you more creative control.

---

## ✨ Features

- **Easy Spline Selection:**  
  Select any GameObject with a compatible spline component in your scene.

- **Prefab Placement:**  
  Choose a prefab and automatically place instances along the spline at regular intervals.

- **Spacing Control:**  
  Adjust the distance between placed objects to fit your design needs.

- **Randomization Options:**  
  Add random rotation and scale to each placed object for a more natural look.

- **Live Preview:**  
  See a preview of the placement in the editor before committing.

- **Batch Placement & Removal:**  
  Place all objects with one click, and remove them just as easily.

---

## 🛠️ How to Use

1. **Add the Script:**  
   Copy `SplineObjectPlacerEditor.cs` into your project's `Editor` folder.

2. **Open the Tool:**  
   In Unity, select `Tools > Spline Object Placer` from the menu bar.

3. **Set Up Your Spline:**  
   Select a GameObject in your scene that has a spline component (such as a custom spline or Unity's built-in splines).

4. **Choose a Prefab:**  
   Drag a prefab from your project into the "Prefab" field in the tool window.

5. **Adjust Settings:**  
   Set the spacing, randomization options, and any other parameters.

6. **Preview & Place:**  
   Click "Preview" to see how objects will be placed.  
   Click "Place Objects" to instantiate them along the spline.

7. **Remove Objects:**  
   Use the "Remove Objects" button to clear all placed instances.

---

## 🎮 Use Cases

- Quickly lay out fences, guardrails, or walls along a path.
- Populate roads or tracks with props, lights, or signs.
- Place trees, rocks, or other environment assets along winding trails.
- Prototype gameplay elements that follow a spline.

---

## 📦 Requirements

- Unity 2021.2 or newer recommended.
- A spline component on your target GameObject (custom or Unity's).

---

## 🚧 Future Plans

- Support for multiple prefabs and weighted random selection.
- Advanced alignment and orientation controls.
- Support for more spline types and third-party spline assets.
- Undo/redo integration for safer editing.

---

## 🤝 Contributing

Found a bug or have a feature request?  
Open an issue or submit a pull request on [GitHub](https://github.com/aranahmed/UnityTools).

---

## 📄 License

MIT License

**Author:** Aran Ahmed
