interface AnchorTogglesProps {
  anchors: string[];
  selectedAnchors: string[];
  onToggleAnchor: (anchor: string) => void;
  onClear: () => void;
}

export default function AnchorToggles({
  anchors,
  selectedAnchors,
  onToggleAnchor,
  onClear,
}: AnchorTogglesProps) {
  return (
    <div className="anchor-toggles">
      <button className={`pill ${selectedAnchors.length === 0 ? "active" : ""}`} onClick={onClear}>
        All
      </button>
      {anchors.map((anchor) => {
        const active = selectedAnchors.includes(anchor);
        return (
          <button
            key={anchor}
            className={`pill ${active ? "active" : ""}`}
            onClick={() => onToggleAnchor(anchor)}
          >
            Anchor {anchor}
          </button>
        );
      })}
    </div>
  );
}
