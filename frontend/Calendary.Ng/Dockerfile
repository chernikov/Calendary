# Stage 1: Node
FROM node:20 AS node
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
CMD ["ng", "serve", "--host", "0.0.0.0"]
