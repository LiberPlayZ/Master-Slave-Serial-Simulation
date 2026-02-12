import { useRef } from "react";

interface ReplayUploadProps {
  onLoadFiles: (files: File[]) => Promise<void>;
  disabled: boolean;
  onLoadGateway: () => Promise<void>;
}

export default function ReplayUpload({ onLoadFiles, disabled, onLoadGateway }: ReplayUploadProps) {
  const inputRef = useRef<HTMLInputElement | null>(null);

  const handleChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files ?? []);
    if (files.length === 0) return;
    await onLoadFiles(files);
    if (inputRef.current) {
      inputRef.current.value = "";
    }
  };

  return (
    <div className="upload">
      <div className="upload-actions">
        <label className={`pill ${disabled ? "disabled" : ""}`}>
          Upload CSV
          <input
            ref={inputRef}
            type="file"
            accept=".csv,text/csv"
            multiple
            disabled={disabled}
            onChange={handleChange}
          />
        </label>
        <button className="pill" onClick={onLoadGateway} disabled={disabled}>
          Load Gateway Logs
        </button>
      </div>
      <p className="upload-hint">Load distance/position CSV logs for replay.</p>
    </div>
  );
}
