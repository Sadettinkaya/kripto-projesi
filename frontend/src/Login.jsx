import React, { useState } from "react";
import { useAuth } from "./AuthContext";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const { login } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    try {
      const res = await fetch("http://localhost:5126/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });
      let data = null;
      try {
        data = await res.json();
      } catch {
        setError("Sunucu JSON cevabı alınamadı");
        return;
      }
      if (res.ok && data.token) {
        login(data.token);
        window.location.href = "/";
      } else {
        setError(typeof data === "string" ? data : (data.message || "Giriş başarısız"));
      }
    } catch (err) {
      setError("Sunucu hatası");
    }
  };

  return (
    <div className="card shadow p-4 bg-white rounded mx-auto" style={{ maxWidth: 400 }}>
      <h2 className="mb-4 text-center">Giriş Yap</h2>
      <form onSubmit={handleSubmit} className="d-flex flex-column gap-3">
        <div className="form-group text-start">
          <label className="mb-1">Email</label>
          <input
            type="email"
            className="form-control"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div className="form-group text-start">
          <label className="mb-1">Şifre</label>
          <input
            type="password"
            className="form-control"
            placeholder="Şifre"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <button type="submit" className="btn btn-primary w-100 mt-2">Giriş Yap</button>
      </form>
      {error && <div className="alert alert-danger mt-3">{error}</div>}
    </div>
  );
}