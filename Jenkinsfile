pipeline {
  agent { label 'docker' }

  options {
    timestamps()
    disableConcurrentBuilds()
  }

  environment {
    REGISTRY_HOST    = 'registry.lunacore.space'
    IMAGE_NAME       = 'btsutskiridze/tsutskiridze-tradebuddy'
    COMPOSE_FILE     = 'docker-compose.yml'
    COMPOSE_SERVICE  = 'tradebuddy'

    SOLUTION         = 'Tsutskiridze.TradeBuddy.sln'
    DOTNET_SDK_IMAGE = 'mcr.microsoft.com/dotnet/sdk:8.0'

    // will be computed
    IMAGE_TAG    = ''
    FULL_IMAGE   = ''
    LATEST_IMAGE = ''
    PUSH_LATEST  = 'false'
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Validate repo layout') {
      steps {
        sh '''
          set -e
          test -f "${SOLUTION}"     || (echo "Missing ${SOLUTION}"; exit 1)
          test -f "${COMPOSE_FILE}" || (echo "Missing ${COMPOSE_FILE}"; exit 1)
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
            ${DOTNET_SDK_IMAGE} \
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
          // Prefer Jenkins-provided commit hash when available
          def commitFull = env.GIT_COMMIT?.trim()
          if (!commitFull) {
            commitFull = sh(script: 'git rev-parse HEAD', returnStdout: true).trim()
          }
          def shortCommit = commitFull.take(8)
    
          def safeBranch = (env.BRANCH_NAME ?: 'local')
            .replaceAll('[^a-zA-Z0-9_.-]', '-')
            .toLowerCase()
    
          env.IMAGE_TAG  = "${safeBranch}-${env.BUILD_NUMBER}-${shortCommit}"
          env.FULL_IMAGE = "${env.REGISTRY_HOST}/${env.IMAGE_NAME}:${env.IMAGE_TAG}"
    
          env.PUSH_LATEST  = (safeBranch == 'main' || safeBranch == 'master') ? 'true' : 'false'
          env.LATEST_IMAGE = (env.PUSH_LATEST == 'true') ? "${env.REGISTRY_HOST}/${env.IMAGE_NAME}:latest" : ""
    
          if (!env.IMAGE_TAG?.trim()) { error("IMAGE_TAG is empty") }
        }
    
        sh '''
          set -e
          echo "BRANCH_NAME=${BRANCH_NAME}"
          echo "GIT_COMMIT=${GIT_COMMIT}"
          echo "IMAGE_TAG=${IMAGE_TAG}"
          echo "FULL_IMAGE=${FULL_IMAGE}"
          git rev-parse --is-inside-work-tree
          git status --porcelain || true
        '''
      }
    }


    stage('Compose Build') {
      steps {
        sh '''
          set -e
          [ -n "${IMAGE_TAG}" ] || (echo "IMAGE_TAG is empty"; exit 1)

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

        # remove the just-built images (env vars set in Compute Image Tags)
        if [ -n "${FULL_IMAGE}" ]; then docker image rm -f "${FULL_IMAGE}" 2>/dev/null || true; fi
        if [ -n "${LATEST_IMAGE}" ]; then docker image rm -f "${LATEST_IMAGE}" 2>/dev/null || true; fi

        # aggressive pruning (space-focused)
        docker container prune -f || true
        docker image prune -af || true
        docker builder prune -af || true
        docker network prune -f || true
      '''

      cleanWs(deleteDirs: true, disableDeferredWipeout: true)
    }
  }
}
