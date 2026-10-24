FROM node:24-bookworm-slim
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates curl git libicu72 libkrb5-3 libssl3 zlib1g && rm -rf /var/lib/apt/lists/* && useradd --uid 20002 --create-home runner
WORKDIR /runner
RUN curl --fail --location --output /tmp/runner.tar.gz https://github.com/actions/runner/releases/download/v2.338.1/actions-runner-linux-x64-2.338.1.tar.gz && echo '08e9ccb3604d2462c24b5e33604dc30810296ca15cc65f23cffd4317eb144c37  /tmp/runner.tar.gz' | sha256sum --check && tar -xzf /tmp/runner.tar.gz && rm /tmp/runner.tar.gz && chown -R 20002:20002 /runner
COPY bridge/sofa.mjs /usr/local/bin/lighthouse-host-bridge
RUN chmod 755 /usr/local/bin/lighthouse-host-bridge
ENV HOME=/home/runner
USER 20002:20002
CMD ["./run.sh"]
