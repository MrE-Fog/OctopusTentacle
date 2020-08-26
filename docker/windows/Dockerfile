ARG SERVERCORE_VERSION
ARG BASE_IMAGE
FROM ${BASE_IMAGE}:${SERVERCORE_VERSION}

ARG BUILD_NUMBER
ARG BUILD_DATE

ENV TentacleVersion ${BUILD_NUMBER}

LABEL \
    org.label-schema.schema-version="1.0" \
    org.label-schema.name="Octopus Deploy Tentacle" \
    org.label-schema.vendor="Octopus Deploy" \
    org.label-schema.url="https://octopus.com" \
    org.label-schema.vcs-url="https://github.com/OctopusDeploy/OctopusTentacle" \
    org.label-schema.license="Apache"  \
    org.label-schema.description="Octopus Tentacle instance with auto-registration to Octopus Server" \
    org.label-schema.version=${BUILD_NUMBER} \
    org.label-schema.build-date=${BUILD_DATE}

EXPOSE 10933

WORKDIR /tmp
RUN \
    curl --fail --silent -O https://s3-ap-southeast-1.amazonaws.com/octopus-testing/tentacle/Octopus.Tentacle.%BUILD_NUMBER%-x64.msi && \
    msiexec /i Octopus.Tentacle.%BUILD_NUMBER%-x64.msi /qn /l*v Tentacle-Installation.log && \
    del /f Octopus.Tentacle.%BUILD_NUMBER%-x64.msi

RUN setx path "%path%;C:\Program Files\Octopus Deploy\Tentacle"
RUN tentacle create-instance --console --instance Tentacle --config C:\Octopus\Tentacle.config

WORKDIR /Scripts
COPY docker/windows/Scripts/*.ps1 /Scripts/
HEALTHCHECK --interval=20s --timeout=20s --retries=6 CMD powershell -file ./healthcheck-tentacle.ps1
ENTRYPOINT powershell -File ./configure-tentacle.ps1 -Verbose  && powershell -File ./run-tentacle.ps1 -Verbose