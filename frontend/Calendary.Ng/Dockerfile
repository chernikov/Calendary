# Development Dockerfile for Calendary.Ng
FROM node:20-alpine AS development

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install --legacy-peer-deps

# Copy application source
COPY . .

# Expose port
EXPOSE 4200

# Start development server
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
