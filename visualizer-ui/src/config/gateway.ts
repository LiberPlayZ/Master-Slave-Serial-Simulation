const DEFAULT_GATEWAY_URL = "http://localhost:5080";

export function getGatewayBaseUrl(): string {
  const envUrl = import.meta.env.VITE_GATEWAY_URL as string | undefined;
  const value = envUrl && envUrl.trim().length > 0 ? envUrl.trim() : DEFAULT_GATEWAY_URL;
  return value.endsWith("/") ? value.slice(0, -1) : value;
}

export function getGatewayWsUrl(): string {
  const baseUrl = getGatewayBaseUrl();
  if (baseUrl.startsWith("https://")) {
    return `${baseUrl.replace("https://", "wss://")}/ws`;
  }
  if (baseUrl.startsWith("http://")) {
    return `${baseUrl.replace("http://", "ws://")}/ws`;
  }
  return `ws://${baseUrl}/ws`;
}
