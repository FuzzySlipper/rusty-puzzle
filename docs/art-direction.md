# Art direction

The designer's direction: **art nouveau fantasy, warm and lush**. Not
necessarily cozy, never grim. Art is 2D: static images drawn by the Engine as
sprites and textures on the board, with simple squash-and-stretch for life.
The DOM interface gets the same look through framing and colour, not through
painted words.

The shared look and the rendering treatment live in the editable
[prompt recipes](art/prompts/README.md); this document holds what the board
needs from any art and how to judge it.

## Readability comes first

The board is a puzzle of laws and positions. The art must keep every fact the
player reasons with obvious at a glance:

- **The checkerboard stays the strongest pattern on the floor.** Rogues and
  rangers keep to one parity class, and players reason by square colour.
  Floor art keeps low internal contrast; the shade applied to alternate cells
  (`checkerShade` in `content/tuning/board-view.json`) carries the parity.
- **Members are distinct by silhouette and hue, not colour alone.** Each
  member has a dominant hue matching its `look.colour` and one bold
  identifying prop that changes its outline (the fighter's round shield, the
  ranger's tall bow, the rogue's crossed daggers, the mage's staff, the
  scholar's open book). A sprite reads when about 80 pixels tall.
- **Board marks stay apart from tiles and pieces.** Exits are a distinct,
  brighter, more ornamented tile. Selection, legal targets and move previews
  are geometric markers in bright flat colour, separate from the organic,
  muted ornament of tiles and the figures of pieces.
- **The camera is orthographic.** Tiles are drawn straight down with no
  perspective; sprites are drawn to stand on or lie on a cell under that
  camera.

Lush means rich colour and ornament at the frame and edges, calm at the
centres where pieces and marks stand. Ornament that competes with a piece or a
marker is wrong however beautiful.

## Never

- Text, letters, numerals or pseudo-writing in any image. Names, rules and
  labels are interface text from `content/interface/`.
- Grimness: skulls, gore, rot, decay, sickly or murky palettes.
- Imported art. Everything is generated from these recipes; nothing is taken
  from donor games, illustrators or third-party packs, and no prompt names a
  living artist or an existing game.

## Motion

Pieces come alive with simple squash-and-stretch around their base: a settle
when placed, a hop when moved, a small lift when selected. It is presentation
only: the board state changes at once, and motion never delays or gates a
command.

## Judging art

Judge in the running game at the window size players use, not as loose
images:

1. A full room with every member present: can each be named by outline alone
   (squint, or view in greyscale)?
2. Select the rogue: is it obvious which squares share its colour, and which
   squares are its targets?
3. Exits next to floor next to walls: is the destination obvious without
   reading the HUD?
4. Hover a mage swap: do the preview tokens read over both checker shades?
5. Panel beside the board: does light text stay readable on it?

## Workflow

Compose prompts from the recipes, generate, check alpha, tiling and stray
lettering, then judge in game. Recipes, the chosen treatment and adopted
images live in git: recipes under `docs/art/prompts/`, runtime images under
`content/`. Generation attempts, probes and assessments live in Den. Assets
under `docs/art/` are not staged into the product.
