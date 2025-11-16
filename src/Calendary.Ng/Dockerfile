FROM node:22 as build-env

WORKDIR /app

RUN npm install -g npm@11.6.2

COPY . .
RUN npm ci --no-audit --no-progress
RUN npm run build

FROM node:22-alpine

WORKDIR /app

# Copy built application
COPY --from=build-env /app/dist/calendary.ng /app

# Install only production dependencies needed for SSR
RUN npm install express

EXPOSE 4000

# Run SSR server
CMD ["node", "server/server.mjs"]
