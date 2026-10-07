# Asset contract: tile texture

One square board cell surface, seen straight down, filling the canvas edge to
edge with no perspective, frame shadow or vignette.

- It tiles with copies of itself on all four sides; ornament stays inside or
  runs continuously across edges.
- The engine darkens alternate cells for the checkerboard, so draw a single
  medium-light value and let the shade be applied; keep value contrast inside
  the tile low so the checkerboard stays the strongest pattern.
- Low detail at the centre: pieces, markers and previews stand on it.
- Square canvas, about 1024 × 1024, opaque.
