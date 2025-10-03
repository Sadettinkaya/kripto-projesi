import React from "react";

export default function CoinDetailModal({ symbol, details, onClose, isLoggedIn, onLoginRedirect }) {
  if (!symbol) return null;

  return (
    <div className="modal" style={{ position: "fixed", top: 0, left: 0, width: "100vw", height: "100vh", background: "rgba(0,0,0,0.5)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 999 }}>
      <div className="card shadow p-4 bg-white rounded" style={{ minWidth: 320, maxWidth: 400, position: "relative" }}>
        <button type="button" className="btn-close position-absolute" style={{ top: 16, right: 16 }} aria-label="Kapat" onClick={onClose}></button>
        <h3 className="mb-4 text-center">{symbol} Detayları</h3>
        {!isLoggedIn ? (
          <div className="text-center">
            <p className="mb-3">Detayları görmek için giriş yapmalısınız.</p>
            <button className="btn btn-primary" onClick={onLoginRedirect}>Giriş Yap</button>
          </div>
        ) : (
          <div className="text-center">
            <p><strong>Anlık Fiyat:</strong> {details.price}</p>
            <p><strong>24 Saatlik En Yüksek:</strong> {details.high24h}</p>
            <p><strong>24 Saatlik Hacim:</strong> {details.volume24h}</p>
            {/* Diğer detaylar eklenebilir */}
          </div>
        )}
      </div>
    </div>
  );
}