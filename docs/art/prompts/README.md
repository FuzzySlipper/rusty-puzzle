# Image prompt recipes

Generation inputs for [the art direction](../../art-direction.md): plain,
editable prompt text for the GPT image tool (the `codex-image-gen` skill on
this workstation), not a runtime loader.

## Compose a request

Concatenate, in this order:

1. [`base-style.md`](base-style.md), the shared direction.
2. **One** treatment: [`treatment-poster-line.md`](treatment-poster-line.md),
   [`treatment-gouache.md`](treatment-gouache.md) or
   [`treatment-stained-glass.md`](treatment-stained-glass.md).
3. **One** asset contract: [`character-sprite.md`](character-sprite.md),
   [`tile-texture.md`](tile-texture.md), [`ui-panel.md`](ui-panel.md) or
   [`marker-glyph.md`](marker-glyph.md).
4. One subject brief below, or an equally concrete new one.

The asset contract overrides general style language on background, view,
framing and size. Keep a batch on one treatment and change one meaningful
variable per comparison. When a set already exists, pass a finished asset of
the same kind as a style reference so new subjects match it. Prompt text is
not proof: check the result for alpha, tiling, silhouette and stray lettering
before using it.

## Subject briefs

Each member's dominant hue matches its `look.colour` in `content/party/`, so
the sprite, the interface and the colour fallback agree.

### Fighter

> A steadfast young knight in a madder-red surcoat over warm steel plate, a
> broad round shield decorated with a stylised lily, a short sword held low.
> Sturdy, grounded, square-shouldered silhouette; the round shield is the
> identifying prop.

### Ranger

> A lithe ranger in a leaf-green hooded cloak with long flowing hems, a tall
> curved longbow held upright beside them, a quiver of fletched arrows. Tall,
> narrow silhouette dominated by the vertical bow.

### Rogue

> A nimble rogue in honey-gold and umber layered leathers, a short cape cut
> in a diagonal sweep, a pair of slim curved daggers held crossed. Leaning,
> diagonal silhouette; the crossed daggers are the identifying prop.

### Mage

> A serene mage in lapis-blue robes embroidered with peacock-eye motifs, a
> tall staff topped by a glowing crystal set in curling silver vines. Long
> robe silhouette widening to the hem; the staff is the identifying prop.

### Scholar

> A kindly scholar in plum and rose layered robes with a long scarf, an open
> book held in one arm and a quill in the other hand, small round spectacles.
> Rounded, soft silhouette; the open book is the identifying prop.

### Floor tile

> A pale honey-sandstone floor slab with a faint inlaid art nouveau border of
> thin curving vines just inside its edges, and a calm, nearly plain centre.

### Exit tile

> A floor slab of deep teal and peacock-green mosaic with a gilded
> sunburst-and-lily inlay radiating from the centre, clearly a destination,
> brighter and more ornamented than an ordinary floor slab.

### Wall tile

> The top of a low wall seen from above: warm umber carved stone capped with
> trailing ivy and small blossoms, darker and heavier than the floor.

### Panel corner

> The top-left corner of an ornamental frame in gilded ironwork: a whiplash
> vine curling into an iris blossom at the corner, two straight gilded bars
> running right and down from it, enclosing a deep plum-umber panel interior.
