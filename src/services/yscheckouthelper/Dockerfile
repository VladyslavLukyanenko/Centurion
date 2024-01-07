FROM adoptopenjdk/openjdk16 AS build
COPY . /build
WORKDIR /build/src/services/yscheckouthelper
RUN chmod +x ./gradlew
RUN ./gradlew clean
RUN ./gradlew generateProto shadowJar

FROM adoptopenjdk/openjdk16
MAINTAINER 0xFF <0xFF@centurion.gg>

WORKDIR /app

COPY --from=build /build/src/services/yscheckouthelper/build/libs/*.jar /app/ysch.jar

ENTRYPOINT ["java", "-XX:+UnlockExperimentalVMOptions", "-agentlib:jdwp=transport=dt_socket,server=y,suspend=n,address=8000", "-Djava.security.egd=file:/dev/./urandom", "-jar", "/app/ysch.jar"]
