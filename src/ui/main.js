/**
 * DOM companion for the board. The Engine draws the board and owns the canvas,
 * input delivery and projection transport; this module shows the projected room,
 * selection and party, and sends back the commands the projection hands it. Every
 * word comes from the projection, and it keeps no board state of its own.
 */
export function mountProductUi(root, context) {
  const panel = element('aside', { className: 'rusty-puzzle-hud' });
  panel.style.cssText = [
    'position:absolute', 'top:12px', 'left:12px', 'max-width:240px', 'padding:12px 14px',
    'background:rgba(12,12,18,0.82)', 'color:#eceae4', 'font:14px/1.4 system-ui,sans-serif',
    'border-radius:8px',
  ].join(';');

  const title = element('h1');
  title.style.cssText = 'font-size:16px;margin:0 0 8px';
  const facts = element('dl');
  facts.style.cssText = 'display:grid;grid-template-columns:auto 1fr;gap:2px 10px;margin:0 0 8px';
  const roomLabel = element('dt');
  const room = element('dd');
  const selectedLabel = element('dt');
  const selected = element('dd');
  for (const value of [room, selected]) value.style.margin = '0';
  facts.append(roomLabel, room, selectedLabel, selected);

  const status = element('p', { id: 'rusty-puzzle-status' });
  status.setAttribute('role', 'status');
  status.setAttribute('aria-live', 'polite');
  status.style.margin = '0 0 8px';

  const partyHeading = element('h2', { id: 'rusty-puzzle-party' });
  partyHeading.style.cssText = 'font-size:14px;margin:0 0 4px';
  const party = element('ul');
  party.setAttribute('aria-labelledby', partyHeading.id);
  party.style.cssText = 'list-style:none;margin:0;padding:0;display:grid;gap:4px';

  panel.append(title, facts, status, partyHeading, party);
  root.append(panel);

  let intent = null;
  const send = (command) => {
    if (intent === null) return;
    context?.intents?.claim?.(intent.id, { kind: 'product-payload', contract: intent.contract, data: command });
  };

  const show = (value) => {
    intent = value.intent;
    title.textContent = value.title;
    roomLabel.textContent = value.labels.room;
    room.textContent = value.room;
    selectedLabel.textContent = value.labels.selected;
    selected.textContent = value.selected;
    status.textContent = value.status;
    partyHeading.textContent = value.labels.party;
    party.replaceChildren(...value.party.map((member) => {
      const button = element('button', { type: 'button', textContent: member.place });
      button.setAttribute('aria-label', member.label);
      button.setAttribute('aria-pressed', String(member.selected));
      button.dataset.rustyPuzzleMember = member.id;
      button.style.cssText = 'width:100%;text-align:left;padding:4px 8px;font:inherit;border-radius:4px;'
        + `border:1px solid ${member.selected ? '#ffd84d' : '#555'};background:${member.selected ? '#3a3420' : '#22222a'};color:inherit`;
      button.addEventListener('click', () => send(member.command));
      const item = element('li');
      item.append(button);
      return item;
    }));
  };

  const unsubscribe = context?.projection?.subscribe?.((envelope) => {
    if (envelope?.value) show(envelope.value);
  });

  return Object.freeze({
    dispose: () => {
      unsubscribe?.();
      panel.remove();
    },
  });
}

function element(tag, properties = {}) {
  return Object.assign(document.createElement(tag), properties);
}
