
import { useEffect, useState } from "react";

export default function useCryptoPrices() {
  const [prices, setPrices] = useState({});

  useEffect(() => {
    const ws = new WebSocket("ws://localhost:5126/ws/prices"); // Backend portunu kontrol et!

    ws.onmessage = (event) => {
      setPrices(JSON.parse(event.data));
    };

    return () => ws.close();
  }, []);

  return prices;
}