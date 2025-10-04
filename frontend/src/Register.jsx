import React, { useState } from "react";

export default function Register() {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    try {
      const res = await fetch("http://localhost:5126/api/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, email, password }),
      });
      if (res.ok) {
        // Backend'den string response geliyor, JSON parse etmeye gerek yok
        const responseText = await res.text();
        setSuccess(responseText || "Kayıt başarılı! Giriş yapabilirsiniz.");
      } else {
        // Hata durumunda JSON parse etmeyi dene
        try {
          const data = await res.json();
          setError(typeof data === "string" ? data : (data.message || "Kayıt başarısız"));
        } catch {
          setError("Kayıt başarısız");
        }
        console.error("Register response:", res.status);
      }
    } catch (err) {
      setError("Network hatası: " + err.message);
      console.error("Register network error:", err);
    }
  };

  return (
    <div className="card shadow p-4 bg-white rounded mx-auto" style={{ maxWidth: 400 }}>
      <h2 className="mb-4 text-center">Kayıt Ol</h2>
      <form onSubmit={handleSubmit} className="d-flex flex-column gap-3">
        <div className="form-group text-start">
          <label className="mb-1">Kullanıcı Adı</label>
          <input
            type="text"
            className="form-control"
            placeholder="Kullanıcı Adı"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
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
        <button type="submit" className="btn btn-primary w-100 mt-2">Kayıt Ol</button>
      </form>
      {error && <div className="alert alert-danger mt-3">{error}</div>}
      {success && <div className="alert alert-success mt-3">{success}</div>}
    </div>
  );
}