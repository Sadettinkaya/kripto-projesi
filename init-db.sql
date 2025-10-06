CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) UNIQUE NOT NULL,
    "Username" VARCHAR(255) NOT NULL,
    "PasswordHash" TEXT NOT NULL
);

