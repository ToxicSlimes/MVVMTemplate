export function SettingsScreenReference() {
  return (
    <div className="settings-overlay" data-screen-spec="settings">
      <div className="settings-panel panel-pixel">
        <h1 className="screen-title">SETTINGS</h1>
        <div className="settings-tabs">
          <button type="button" data-name="tab-audio">Audio</button>
          <button type="button" data-name="tab-video">Video</button>
        </div>
        <div data-tab="audio">
          <label>Master</label>
          <input type="range" data-name="master-volume" min={0} max={1} step={0.01} />
        </div>
        <button type="button" data-name="apply-button">Apply</button>
      </div>
    </div>
  );
}
