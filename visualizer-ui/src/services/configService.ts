import type { VisualizerConfig } from "../models/visualizerConfig";

export async function fetchConfig(baseUrl: string): Promise<VisualizerConfig | null> {
  try {
    const response = await fetch(`${baseUrl}/config`);
    if (!response.ok) return null;
    const data = (await response.json()) as VisualizerConfig;
    return data;
  } catch {
    return null;
  }
}
