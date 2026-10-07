# Asset contract: character sprite

One party member as a 2D sprite drawn on the board with gentle
squash-and-stretch. The board is seen from above by an orthographic camera.

- **Standing figure** (default): full body, front view facing the viewer,
  feet on one baseline near the bottom edge, whole figure in frame with a
  small margin. A neutral, balanced pose that can squash and stretch around
  the feet without looking broken.
- **Medallion token** (alternative): the character's head and shoulders, or
  bust with its identifying prop, inside a round ornamental medallion seen
  straight-on, as a game piece lying on the board.
- Real transparency (alpha) around the figure or medallion, never a painted
  checkerboard pattern standing in for it. No ground, cast shadow, scenery,
  frame or vignette beyond the medallion itself.
- Readable when drawn about 80 pixels tall: a distinctive silhouette and a
  dominant identifying hue, plus one bold prop.
- Square canvas, about 1024 × 1024.
