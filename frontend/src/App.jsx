import React from "react";
import CryptoPrices from "./CryptoPrices";
import Login from "./Login";
import Register from "./Register";
import { AuthProvider, useAuth } from "./AuthContext";

function getPage() {
  const path = window.location.pathname;
  if (path === "/login") return "login";
  if (path === "/register") return "register";
  return "home";
}

function AppContent() {
  const page = getPage();
  const { user, logout } = useAuth();

  return (
    
    <div className="d-flex flex-column justify-content-center align-items-center min-vh-100" style={{ background: '#f8f9fa' }}>
      <nav className="mb-4 text-center">
        <a className="btn btn-link text-primary" href="/">Ana Sayfa</a>
        {user ? (
          <>
            <span className="text-muted me-3">Hoş geldiniz!</span>
            <button className="btn btn-outline-danger btn-sm" onClick={logout}>
              Çıkış Yap
            </button>
          </>
        ) : (
          <>
            <a className="btn btn-link text-primary" href="/login">Giriş Yap</a>
            <a className="btn btn-link text-primary" href="/register">Kayıt Ol</a>
          </>
        )}
      </nav>
      <div className="w-100 d-flex justify-content-center">
        <div style={{ minWidth: 320, maxWidth: 350, width: '100%' }}>
          {page === "home" && <CryptoPrices />}
          {page === "login" && <Login />}
          {page === "register" && <Register />}
        </div>
      </div>
    </div>
  );
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;