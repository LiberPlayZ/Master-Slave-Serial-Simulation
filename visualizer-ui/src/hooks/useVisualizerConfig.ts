import { useEffect, useState } from "react";
import { fetchConfig } from "../services/configService";

interface ConfigLabels {
  noise: string;
  jitter: string;
}

export function useVisualizerConfig(baseUrl: string): ConfigLabels {
  const [labels, setLabels] = useState<ConfigLabels>({
    noise: "n/a",
    jitter: "n/a",
  });

  useEffect(() => {
    const loadConfig = async () => {
      const data = await fetchConfig(baseUrl);
      if (!data) return;
      setLabels({
        noise: `${data.distanceNoiseMin} → ${data.distanceNoiseMax}`,
        jitter: `${data.responseJitterMs} ms`,
      });
    };

    loadConfig();
  }, [baseUrl]);

  return labels;
}
