# Build context: monorepo root (el lockfile pnpm vive en la raiz).
FROM node:22-alpine AS builder
RUN corepack enable
WORKDIR /app
# Manifiestos primero para cachear la instalacion de dependencias.
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY frontend/package.json ./frontend/package.json
RUN pnpm install --frozen-lockfile
COPY frontend/ ./frontend/
WORKDIR /app/frontend
# Next.js inyecta las NEXT_PUBLIC_* en build-time; deben estar presentes antes de `build`.
ARG NEXT_PUBLIC_API_BASE_URL=http://localhost:40303
ENV NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL
RUN pnpm run build

FROM nginx:1.27-alpine AS runtime
# `output: "export"` deja el sitio estatico en `out/`.
COPY --from=builder /app/frontend/out /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s \
  CMD wget -qO- http://localhost/ || exit 1
