import { useRef } from "react";

interface ReplayUploadProps {
  onLoadFiles: (files: File[]) => Promise<void>;
  disabled: boolean;
}

export default function ReplayUpload({ onLoadFiles, disabled }: ReplayUploadProps) {
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
      <p className="upload-hint">Load distance/position CSV logs for replay.</p>
    </div>
  );
}
