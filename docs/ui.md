# DOM companion

`src/ui/main.js` exports `mountProductUi`. It observes the Engine-delivered
`rusty.puzzle.board` projection and shows the room name, the move count, the
selected member and the law it moves by, the Undo, Reset and Next room
controls (disabled as projected), a status sentence for screen readers
(`role="status"`), one button per party member and one per legal move. A button
sends the command its projection entry carries, through the payload intent the
projection names (`intent.id`, `intent.contract`). The product project selects
this directory and module for SDK staging.

The module also places an empty element beside the panel and anchors it under
the name the projection gives (`view.anchor`) with `context.viewport.anchor`;
the Engine draws the board inside it, so the board never sits under the panel.
The element is not interactive, so presses on it reach the canvas.

Every word on screen comes from the projection, which C# composes from
`content/interface/text.json`. The DOM holds no board state, action names or
command fields: a new command, control or label is a C# and content change only.

Keep only browser assets in `src/ui/`. The host admits every staged file by its
content type; documentation belongs under `docs/`.

Keep this lane to DOM presentation, accessibility, and semantic actions.
The board is drawn by the Engine from C#; board state, input delivery,
projection transport, the canvas and rendering belong to C# and the Engine.
Dispose subscriptions and remove the panel when the host unmounts the UI.
