pipeline {
  agent { label 'docker' }

  options {
    timestamps()
    disableConcurrentBuilds()
  }

  environment {
    REGISTRY_HOST    = 'registry.lunacore.space'
    IMAGE_NAME       = 'btsutskiridze/tsutskiridze-tradebuddy' // keep lowercase
    IMAGE_TAG        = ''   // computed later
    COMPOSE_FILE     = 'docker-compose.yml'
    COMPOSE_SERVICE  = 'tradebuddy'

    SOLUTION         = 'Tsutskiridze.TradeBuddy.sln'
    DOTNET_SDK_IMAGE = 'mcr.microsoft.com/dotnet/sdk:8.0'
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Validate repo layout') {
      steps {
        sh '''
          set -e
          test -f "${SOLUTION}"     || (echo "Missing ${SOLUTION}"; exit 1)
          test -f "${COMPOSE_FILE}" || (echo "Missing ${COMPOSE_FILE}"; exit 1)

          echo "Repo layout OK:"
          ls -la
        '''
      }
    }

    stage('Dotnet: Restore + Build + Test') {
      steps {
        sh '''
          set -e
          docker run --rm \
            -v "${WORKSPACE}":/src:rw -w /src \
            mcr.microsoft.com/dotnet/sdk:8.0 \
            bash -lc '
              set -e
              dotnet restore "${SOLUTION}"
              dotnet build "${SOLUTION}" -c Release --no-restore
              dotnet test  "${SOLUTION}" -c Release --no-build \
                --logger "trx;LogFileName=test_results.trx" \
                --results-directory "./TestResults"
            '
        '''
      }
      post {
        always {
          archiveArtifacts artifacts: 'TestResults/**', allowEmptyArchive: true
        }
      }
    }


    stage('Compute Image Tags') {
      steps {
        script {
          def shortCommit = sh(script: 'git rev-parse --short=8 HEAD', returnStdout: true).trim()
          def safeBranch  = (env.BRANCH_NAME ?: 'local').replaceAll('[^a-zA-Z0-9_.-]', '-').toLowerCase()
          env.IMAGE_TAG   = "${safeBranch}-${env.BUILD_NUMBER}-${shortCommit}"

          // Optional: latest only for main/master
          env.PUSH_LATEST = (safeBranch == 'main' || safeBranch == 'master') ? 'true' : 'false'
        }

        sh '''
          echo "REGISTRY_HOST=${REGISTRY_HOST}"
          echo "IMAGE_NAME=${IMAGE_NAME}"
          echo "IMAGE_TAG=${IMAGE_TAG}"
          echo "PUSH_LATEST=${PUSH_LATEST}"
        '''
      }
    }

    stage('Compose Build') {
      steps {
        sh '''
          set -e
          export REGISTRY_HOST="${REGISTRY_HOST}"
          export IMAGE_NAME="${IMAGE_NAME}"
          export IMAGE_TAG="${IMAGE_TAG}"

          docker compose -f "${COMPOSE_FILE}" build "${COMPOSE_SERVICE}"
        '''
      }
    }

    stage('Registry Login & Compose Push') {
      steps {
        withCredentials([usernamePassword(
          credentialsId: 'registry-creds',
          usernameVariable: 'REG_USER',
          passwordVariable: 'REG_PASS'
        )]) {
          sh '''
            set -e
            export REGISTRY_HOST="${REGISTRY_HOST}"
            export IMAGE_NAME="${IMAGE_NAME}"
            export IMAGE_TAG="${IMAGE_TAG}"

            echo "$REG_PASS" | docker login "${REGISTRY_HOST}" -u "$REG_USER" --password-stdin

            docker compose -f "${COMPOSE_FILE}" push "${COMPOSE_SERVICE}"

            if [ "${PUSH_LATEST}" = "true" ]; then
              # Re-tag and push latest (without rebuilding)
              FULL_IMAGE="${REGISTRY_HOST}/${IMAGE_NAME}:${IMAGE_TAG}"
              LATEST_IMAGE="${REGISTRY_HOST}/${IMAGE_NAME}:latest"
              docker tag "${FULL_IMAGE}" "${LATEST_IMAGE}"
              docker push "${LATEST_IMAGE}"
            fi
          '''
        }
      }
    }
  }

  post {
    always {
      sh 'docker logout "${REGISTRY_HOST}" || true'
    }
    cleanup {
      sh '''
        set +e
  
        # remove the just-built images
        docker image rm -f "${FULL_IMAGE}" 2>/dev/null || true
        if [ -n "${LATEST_IMAGE}" ]; then docker image rm -f "${LATEST_IMAGE}" 2>/dev/null || true; fi
  
        # aggressive pruning (space-focused)
        docker container prune -f || true
        docker image prune -af || true
        docker builder prune -af || true
        docker network prune -f || true
  
        # OPTIONAL: also remove SDK image every run (max space, slowest builds)
        # docker image rm -f mcr.microsoft.com/dotnet/sdk:8.0 || true
      '''
      
      cleanWs(deleteDirs: true, disableDeferredWipeout: true)
    }
  }
}
