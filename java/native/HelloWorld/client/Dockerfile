# First Stage: Build the application
FROM maven:3.9.9-amazoncorretto-17-alpine AS build
WORKDIR /build
COPY pom.xml .
COPY src ./src
RUN mvn clean package -DskipTests

# Second Stage: Runtime environment (use JRE for a smaller image)
FROM eclipse-temurin:17-jre-alpine
WORKDIR /app
COPY --from=build /build/target/java-client-sample-0.1.0.jar /app/client.jar

# Run the application
ENTRYPOINT ["java", "-jar", "/app/client.jar"]
