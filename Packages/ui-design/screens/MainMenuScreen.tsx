export function MainMenuScreenReference() {
  return (
    <div className="menu-root" data-screen-spec="main-menu">
      <div className="menu-card panel-pixel">
        <h1 className="menu-title title-pixel">MVVM TEMPLATE</h1>
        <button type="button" data-name="counter-button">COUNTER</button>
        <button type="button" data-name="settings-button">SETTINGS</button>
        <button type="button" data-name="multiplayer-button">MULTIPLAYER</button>
      </div>
    </div>
  );
}
