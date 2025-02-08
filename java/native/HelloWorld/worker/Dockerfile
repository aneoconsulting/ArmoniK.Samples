# First Stage: Build the application
FROM maven:3.9.9-amazoncorretto-17-alpine AS build
WORKDIR /build
COPY pom.xml .

COPY src ./src
RUN mvn clean package -DskipTests

# Second Stage: Runtime environment (use JRE for a smaller image)
FROM eclipse-temurin:21-jre-noble
RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser
RUN mkdir /cache /local_storage && chown armonikuser: /cache /local_storage
USER armonikuser
WORKDIR /app
COPY --from=build /build/target/java-worker-sample-0.1.0-shaded.jar /app/worker.jar

# Run the application
ENTRYPOINT ["java", "-jar", "/app/worker.jar"]
