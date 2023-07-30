# This file is part of StreamSDR.
#
# StreamSDR is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# StreamSDR is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with StreamSDR. If not, see <https://www.gnu.org/licenses/>.

###############
## BUILD IMAGE
###############

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim-amd64 AS build

WORKDIR /app

ARG version

ENV MINVERVERSIONOVERRIDE=${version}

COPY ./src ./src
COPY ./assets/icon.ico ./assets/icon.ico

# Publish the app without the app host, which is not required by the runtime docker image
RUN dotnet publish "src/StreamSDR.csproj" /p:UseAppHost=false /p:PublishSingleFile=false -c Release -o /app/publish

#############
## APP IMAGE
#############

FROM mcr.microsoft.com/dotnet/runtime:7.0-bullseye-slim AS app

LABEL org.opencontainers.image.title="StreamSDR"
LABEL org.opencontainers.image.description="Server for software defined radios"
LABEL org.opencontainers.image.url="https://github.com/orryverducci/StreamSDR"
LABEL org.opencontainers.image.source="https://github.com/orryverducci/StreamSDR"
LABEL org.opencontainers.image.vendor="Orry Verducci"
LABEL org.opencontainers.image.licenses="GPL-3.0-only"

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends librtlsdr0 && rm -rf /var/lib/apt/lists/*

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 1234

ENTRYPOINT ["dotnet", "streamsdr.dll"]
