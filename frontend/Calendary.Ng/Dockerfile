# Development Dockerfile for Calendary.Ng Angular application
FROM node:20-alpine

WORKDIR /app

# Install dependencies
COPY package.json package-lock.json* ./
RUN npm ci || npm install

# Copy source code
COPY . .

# Expose Angular development server port
EXPOSE 4200

# Start Angular development server
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "4200"]
