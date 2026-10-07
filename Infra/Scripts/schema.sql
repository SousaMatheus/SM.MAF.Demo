CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE IF NOT EXISTS recomendations (
	    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	    title TEXT NOT NULL,
	    category TEXT NOT NULL,
	    embedding vector (1024) NOT NULL
    );

CREATE INDEX IF NOT EXISTS idx_recomendations ON recomendations USING HNSW (embedding vector_l2_ops);

CREATE TABLE IF NOT EXISTS products (
      id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
      title TEXT NOT NULL,
      category TEXT NOT NULL,
      summary TEXT NOT NULL,
      description TEXT NOT NULL
);
