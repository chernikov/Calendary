# syntax=docker/dockerfile:1.7

###########################
# Stage 1 – Build assets  #
###########################
FROM node:20-alpine AS build
WORKDIR /app

# Disable Angular telemetry and enable deterministic installs
ENV NG_CLI_ANALYTICS=false \
    CI=true

COPY package.json package-lock.json* ./
RUN npm ci

COPY . ./

# Allow overriding build configuration at build time
ARG ANGULAR_CONFIGURATION=production
RUN npm run build -- --configuration ${ANGULAR_CONFIGURATION}

############################################
# Stage 2 – Minimal runtime for SSR server #
############################################
FROM node:20-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production \
    PORT=4000 \
    NG_CLI_ANALYTICS=false

# Install only production dependencies required for Express server
COPY package.json package-lock.json* ./
RUN npm ci --omit=dev

# Copy the compiled browser + server bundles
COPY --from=build /app/dist ./dist

EXPOSE 4000

CMD ["node", "dist/calendary.ng/server/server.mjs"]
