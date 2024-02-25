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

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim-amd64 AS build

WORKDIR /build

ARG version

ENV MINVERVERSIONOVERRIDE=${version}

COPY ./src ./src
COPY --chmod=0755 ./docker/build-scripts ./scripts

RUN /build/scripts/build.sh

#############
## APP IMAGE
#############

FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim AS app

LABEL org.opencontainers.image.title="StreamSDR"
LABEL org.opencontainers.image.description="Server for software defined radios"
LABEL org.opencontainers.image.url="https://streamsdr.io/"
LABEL org.opencontainers.image.source="https://github.com/orryverducci/StreamSDR"
LABEL org.opencontainers.image.vendor="Orry Verducci"
LABEL org.opencontainers.image.licenses="GPL-3.0-only"

WORKDIR /app

ENV S6_KEEP_ENV=1

COPY --from=build /build/s6/output /
COPY --from=build /build/app .
COPY --chmod=0755 ./docker/setup-scripts /tmp/scripts

RUN /tmp/scripts/setup.sh && rm -rf /tmp/*

USER appuser

EXPOSE 1234

ENTRYPOINT ["/init"]

CMD ["dotnet", "streamsdr.dll"]
