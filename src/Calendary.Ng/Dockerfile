FROM node:22 as build-env

WORKDIR /app

RUN npm install -g npm@11.6.2

COPY . .
RUN npm install
RUN npm run build

FROM nginx:alpine
COPY --from=build-env /app/dist/calendary.ng /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80 443
CMD ["nginx", "-g", "daemon off;"]
