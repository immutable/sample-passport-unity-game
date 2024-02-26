FROM node:20.11

LABEL org.opencontainers.image.source=https://github.com/immutable/sample-passport-unity-game

WORKDIR /usr/src/app
COPY . /usr/src/app

RUN npm install

EXPOSE 6060

CMD ["npm", "run", "dev"]
