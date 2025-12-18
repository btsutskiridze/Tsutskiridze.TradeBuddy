pipeline {
  agent { label 'docker-dotnet' }

  options {
    timestamps()
    disableConcurrentBuilds()
    skipDefaultCheckout(true)
  }

  // Keep static env only here (no Groovy method calls)
  environment {
    COMPOSE_FILE_1 = 'docker-compose.yml'
    COMPOSE_FILE_2 = 'docker-compose.ci.yml'
    ENV_FILE       = '.env.ci'
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Compute Compose Project Name') {
      steps {
        script {
          def raw = "ci-${env.JOB_NAME}-${env.BUILD_NUMBER}".toLowerCase()
          // Compose project names should be simple; keep letters/numbers and dashes
          env.COMPOSE_PROJECT_NAME = raw.replaceAll(/[^a-z0-9]+/, '-')
        }
        sh 'echo "COMPOSE_PROJECT_NAME=${COMPOSE_PROJECT_NAME}"'
      }
    }

    stage('Create .env.ci from Jenkins credential') {
      steps {
        sh 'touch .env'
        
        withCredentials([file(credentialsId: 'tradebuddy-env-ci', variable: 'ENVFILE')]) {
          sh '''
            set -e
            cp "$ENVFILE" "${ENV_FILE}"
            chmod 600 "${ENV_FILE}"
            ls -la "${ENV_FILE}"
          '''
        }
      }
    }

    stage('Compose Validate') {
      steps {
        sh '''
          set -e
          docker compose --project-name "${COMPOSE_PROJECT_NAME}" \
            -f "${COMPOSE_FILE_1}" -f "${COMPOSE_FILE_2}" \
            --env-file "${ENV_FILE}" config >/dev/null
        '''
      }
    }

    stage('Compose Build') {
      steps {
        sh '''
          set -e
          docker compose --project-name "${COMPOSE_PROJECT_NAME}" \
            -f "${COMPOSE_FILE_1}" -f "${COMPOSE_FILE_2}" \
            --env-file "${ENV_FILE}" build --pull
        '''
      }
    }

    // Optional: if you want to push as well
    // stage('Compose Push') {
    //   steps {
    //     sh '''
    //       set -e
    //       docker compose --project-name "${COMPOSE_PROJECT_NAME}" \
    //         -f "${COMPOSE_FILE_1}" -f "${COMPOSE_FILE_2}" \
    //         --env-file "${ENV_FILE}" push
    //     '''
    //   }
    // }
  }

  post {
    always {
      sh '''
        set +e
        docker compose --project-name "${COMPOSE_PROJECT_NAME}" \
          -f "${COMPOSE_FILE_1}" -f "${COMPOSE_FILE_2}" \
          --env-file "${ENV_FILE}" down -v --remove-orphans || true

        rm -f "${ENV_FILE}" || true
      '''
      // Wipes workspace contents reliably
      deleteDir()
    }
  }
}
