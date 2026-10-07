# DOM companion

`src/ui/main.js` exports `mountProductUi`. It observes the Engine-delivered
`rusty.puzzle.board` projection and shows the room name, the selected member,
a status sentence for screen readers (`role="status"`) and one button per
party member. A button sends the command its projection entry carries,
through the payload intent the projection names (`intent.id`,
`intent.contract`). The product project selects this directory and module for
SDK staging.

Every word on screen comes from the projection, which C# composes from
`content/interface/text.json`. The DOM holds no board state, action names or
command fields: a new command or label is a C# and content change only.

Keep only browser assets in `src/ui/`. The host admits every staged file by its
content type; documentation belongs under `docs/`.

Keep this lane to DOM presentation, accessibility, and semantic actions.
The board is drawn by the Engine from C#; board state, input delivery,
projection transport, the canvas and rendering belong to C# and the Engine.
Dispose subscriptions and remove the panel when the host unmounts the UI.
