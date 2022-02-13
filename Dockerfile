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

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src

ARG version

ENV MINVERVERSIONOVERRIDE=${version}

COPY ./src .

# Publish the app without the app host, which is not required by the runtime docker image
RUN dotnet publish "StreamSDR.csproj" /p:UseAppHost=false -c Release -o /app/publish

#############
## APP IMAGE
#############

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS app

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends librtlsdr-dev && rm -rf /var/lib/apt/lists/*

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 1234

ENTRYPOINT ["dotnet", "streamsdr.dll"]
