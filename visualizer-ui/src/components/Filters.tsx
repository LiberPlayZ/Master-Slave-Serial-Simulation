import AnchorToggles from "./AnchorToggles";

interface FiltersProps {
  anchorOptions: string[];
  selectedAnchors: string[];
  onToggleAnchor: (anchor: string) => void;
  onClearAnchors: () => void;
  positionRole: string;
  onPositionRoleChange: (value: string) => void;
  positionId: string;
  onPositionIdChange: (value: string) => void;
}

export default function Filters({
  anchorOptions,
  selectedAnchors,
  onToggleAnchor,
  onClearAnchors,
  positionRole,
  onPositionRoleChange,
  positionId,
  onPositionIdChange,
}: FiltersProps) {
  return (
    <div className="filters">
      <div className="field">
        <label>Anchors</label>
        <AnchorToggles
          anchors={anchorOptions}
          selectedAnchors={selectedAnchors}
          onToggleAnchor={onToggleAnchor}
          onClear={onClearAnchors}
        />
      </div>
      <div className="field">
        <label htmlFor="roleFilter">Positions</label>
        <select
          id="roleFilter"
          value={positionRole}
          onChange={(event) => onPositionRoleChange(event.target.value)}
        >
          <option value="all">All roles</option>
          <option value="Pilot">Pilot</option>
          <option value="Anchor">Anchor</option>
        </select>
      </div>
      <div className="field">
        <label htmlFor="idFilter">Id contains</label>
        <input
          id="idFilter"
          type="text"
          placeholder="Anchor id"
          value={positionId}
          onChange={(event) => onPositionIdChange(event.target.value)}
        />
      </div>
    </div>
  );
}
