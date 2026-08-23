FROM node:22-alpine AS build
WORKDIR /src

COPY package.json package-lock.json* ./
RUN npm install

COPY . .
RUN npx ng build --configuration production

FROM nginx:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist/calendary/browser /usr/share/nginx/html

EXPOSE 80
