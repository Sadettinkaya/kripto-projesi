import React, { useState } from "react";
import useCryptoPrices from "./useCryptoPrices";
import CoinDetailModal from "./CoinDetailModal";

function isLoggedIn() {
  return Boolean(localStorage.getItem("token"));
}

export default function CryptoPrices() {
  const prices = useCryptoPrices();
  const [selectedSymbol, setSelectedSymbol] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);

  // Sadece ilk 5 sembolü göster
  const priceEntries = Object.entries(prices).slice(0, 5);

  // Sembole tıklanınca modalı aç
  const handleSymbolClick = (symbol) => {
    setSelectedSymbol(symbol);
    setModalOpen(true);
  };

  // Modalı kapat
  const handleCloseModal = () => {
    setModalOpen(false);
    setSelectedSymbol(null);
  };

  // Login ekranına yönlendir
  const handleLoginRedirect = () => {
    window.location.href = "/login";
  };

  // Sembol detayları örnek (backend'den alınmalı)
  const getDetails = (symbol) => {
    return {
      price: prices[symbol],
      high24h: Math.round(prices[symbol] * 1.05),
      volume24h: Math.round(prices[symbol] * 1000),
    };
  };

  return (
    <div className="card shadow p-4 bg-white rounded">
      <h2 className="mb-4 text-center">Anlık Kripto Fiyatları</h2>
      <ul className="list-group">
        {priceEntries.map(([symbol, price]) => (
          <li key={symbol} className="list-group-item list-group-item-action d-flex justify-content-between align-items-center" style={{ cursor: "pointer" }} onClick={() => handleSymbolClick(symbol)}>
            <span><strong>{symbol}</strong></span>
            <span>{price}</span>
          </li>
        ))}
      </ul>
      {modalOpen && (
        <CoinDetailModal
          symbol={selectedSymbol}
          details={getDetails(selectedSymbol)}
          isLoggedIn={isLoggedIn()}
          onClose={handleCloseModal}
          onLoginRedirect={handleLoginRedirect}
        />
      )}
    </div>
  );
}