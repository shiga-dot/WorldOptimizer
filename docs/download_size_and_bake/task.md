# Protocol: Add Download Size & Light Bake Features

- [x] **Plan**: Design UI and Logic for new features <!-- id: 0 -->
- [x] **Feature**: Implement "Make All Lights Baked" logic <!-- id: 1 -->
    - Iterate all generic Lights (Directional, Point, Spot).
    - Set Mode to Baked.
    - (Optional) Suggest setting objects to Static.
- [x] **Feature**: Implement "Reduce Download Size" logic <!-- id: 2 -->
    - Preset button for "Aggressive Size Reduction".
    - Actions: Max Texture Size -> 1024, Crunch -> On (Quality 50), Audio -> Mono/Vorbis/low quality.
- [x] **UI**: Add these to `DrawAdvancedTab` in `WorldOptimizerWindow.cs` <!-- id: 3 -->
- [x] **Verify**: Compile and check functionality <!-- id: 4 -->
- [ ] **Document**: Update Walkthrough <!-- id: 5 -->
