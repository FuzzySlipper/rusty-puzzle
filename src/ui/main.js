/**
 * DOM companion for the board. The Engine draws the board and owns the canvas,
 * input delivery and projection transport; this module shows the projected room,
 * selection, party, legal moves and controls, and sends back the commands the
 * projection hands it. Every word and every command comes from the projection;
 * it keeps no board state, action names or command fields of its own.
 */
export function mountProductUi(root, context) {
  const panel = element('aside', { className: 'rusty-puzzle-hud' });
  panel.style.cssText = [
    'position:absolute', 'top:12px', 'left:12px', 'width:250px', 'box-sizing:border-box', 'padding:12px 14px',
    'background:rgba(12,12,18,0.82)', 'color:#eceae4', 'font:14px/1.4 system-ui,sans-serif',
    'border-radius:8px',
  ].join(';');

  // The Engine draws the board inside this element, beside the panel. It is not interactive, so
  // presses on it reach the canvas.
  const boardView = element('div', { className: 'rusty-puzzle-board' });
  boardView.style.cssText = 'position:absolute;top:0;bottom:0;left:274px;right:0';

  const title = element('h1');
  title.style.cssText = 'font-size:16px;margin:0 0 8px';
  const facts = element('dl');
  facts.style.cssText = 'display:grid;grid-template-columns:auto 1fr;gap:2px 10px;margin:0 0 8px';
  const fact = () => {
    const label = element('dt');
    const value = element('dd');
    value.style.margin = '0';
    facts.append(label, value);
    return { label, value };
  };
  const roomFact = fact();
  const moveCountFact = fact();
  const selectedFact = fact();
  const lawFact = fact();

  const brief = element('p');
  brief.style.cssText = 'margin:0 0 8px;font-style:italic;opacity:0.9';

  // The room picker: a native select whose options carry the projected room commands.
  const picker = element('label');
  picker.style.cssText = 'display:grid;gap:2px;margin:0 0 8px';
  const pickerLabel = element('span');
  const rooms = element('select');
  rooms.style.cssText = 'font:inherit;padding:2px 4px;background:#22222a;color:inherit;border:1px solid #555;border-radius:4px';
  picker.append(pickerLabel, rooms);
  let roomCommands = [];
  // The room the player last chose, until a projection shows it current: projections that arrive in
  // between describe an earlier choice and must not move the picker back under the player's keys.
  let chosenRoom = null;
  rooms.addEventListener('change', () => {
    chosenRoom = rooms.selectedIndex;
    send(roomCommands[chosenRoom]);
  });

  const controls = element('div');
  controls.style.cssText = 'display:flex;flex-wrap:wrap;gap:6px;margin:0 0 8px';

  const status = element('p', { id: 'rusty-puzzle-status' });
  status.setAttribute('role', 'status');
  status.setAttribute('aria-live', 'polite');
  status.style.margin = '0 0 8px';

  const [partyHeading, party] = section('rusty-puzzle-party');
  const [movesHeading, moves] = section('rusty-puzzle-moves');

  panel.append(title, picker, facts, brief, controls, status, partyHeading, party, movesHeading, moves);
  root.append(boardView, panel);
  let anchored = null;
  let releaseAnchor = () => {};

  let intent = null;
  const send = (command) => {
    if (intent === null) return;
    context?.intents?.claim?.(intent.id, { kind: 'product-payload', contract: intent.contract, data: command });
  };

  const show = (value) => {
    intent = value.intent;
    if (value.view.anchor !== anchored) {
      releaseAnchor();
      anchored = value.view.anchor;
      releaseAnchor = context?.viewport?.anchor?.(anchored, boardView) ?? (() => {});
    }
    title.textContent = value.title;
    pickerLabel.textContent = value.labels.rooms;
    roomCommands = value.rooms.map((room) => room.command);
    const current = value.rooms.findIndex((room) => room.current);
    if (chosenRoom === current) chosenRoom = null;
    const labels = value.rooms.map((room) => room.label);
    if (rooms.options.length !== labels.length || labels.some((label, index) => rooms.options[index].textContent !== label)) {
      rooms.replaceChildren(...labels.map((label) => element('option', { textContent: label })));
    }
    if (chosenRoom === null) rooms.selectedIndex = current;
    setFact(roomFact, value.labels.room, value.room);
    brief.textContent = value.brief;
    setFact(moveCountFact, value.labels.moveCount, String(value.moveCount));
    setFact(selectedFact, value.labels.selected, value.selected);
    // The law row shows only while a member is selected.
    setFact(lawFact, value.labels.law, value.law ?? null);
    status.textContent = value.status;

    controls.replaceChildren(...value.controls.map((control) => {
      const button = commandButton(control.label, control.command, send, false);
      button.disabled = !control.enabled;
      if (button.disabled) button.style.opacity = '0.45';
      button.dataset.rustyPuzzleControl = control.id;
      return button;
    }));

    partyHeading.textContent = value.labels.party;
    party.replaceChildren(...value.party.map((member) => {
      const button = commandButton(`${member.mark} · ${member.place}`, member.command, send, member.selected);
      button.setAttribute('aria-label', member.label);
      button.setAttribute('aria-pressed', String(member.selected));
      button.dataset.rustyPuzzleMember = member.id;
      return listItem(button);
    }));

    movesHeading.hidden = moves.hidden = value.moves.length === 0;
    movesHeading.textContent = value.labels.moves;
    moves.replaceChildren(...value.moves.map((move) => listItem(commandButton(move.label, move.command, send, false))));
  };

  const unsubscribe = context?.projection?.subscribe?.((envelope) => {
    if (envelope?.value) show(envelope.value);
  });

  return Object.freeze({
    dispose: () => {
      unsubscribe?.();
      releaseAnchor();
      boardView.remove();
      panel.remove();
    },
  });
}

function section(id) {
  const heading = element('h2', { id });
  heading.style.cssText = 'font-size:14px;margin:8px 0 4px';
  const list = element('ul');
  list.setAttribute('aria-labelledby', id);
  list.style.cssText = 'list-style:none;margin:0;padding:0;display:grid;gap:4px';
  return [heading, list];
}

function setFact({ label, value }, labelText, valueText) {
  label.hidden = value.hidden = valueText === null;
  label.textContent = labelText;
  value.textContent = valueText ?? '';
}

function commandButton(text, command, send, highlighted) {
  const button = element('button', { type: 'button', textContent: text });
  button.style.cssText = 'text-align:left;padding:4px 8px;font:inherit;border-radius:4px;color:inherit;'
    + `border:1px solid ${highlighted ? '#ffd84d' : '#555'};background:${highlighted ? '#3a3420' : '#22222a'}`;
  button.addEventListener('click', () => send(command));
  return button;
}

function listItem(child) {
  const item = element('li');
  child.style.width = '100%';
  item.append(child);
  return item;
}

function element(tag, properties = {}) {
  return Object.assign(document.createElement(tag), properties);
}
