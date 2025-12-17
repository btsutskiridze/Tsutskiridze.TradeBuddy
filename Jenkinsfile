pipeline {
  agent any

  options {
    timestamps()
    disableConcurrentBuilds()
  }

  environment {
    // Self-hosted registry (no scheme)
    REGISTRY_HOST = 'registry.lunacore.space'

    // Image path in your registry (adjust to your repo naming convention)
    IMAGE_NAME = 'btsutskiridze/Tsutskiridze.TradeBuddy'

    // Dockerfile location (adjust if needed)
    DOCKERFILE_PATH = 'Dockerfile'

    // Optional: solution path (adjust)
    SOLUTION = 'Tsutskiridze.TradeBuddy.sln'

    // Official SDK image for build/test
    DOTNET_SDK_IMAGE = 'mcr.microsoft.com/dotnet/sdk:8.0'
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Restore') {
      steps {
        sh '''
          docker run --rm \
            -v "$PWD":/src -w /src \
            ${DOTNET_SDK_IMAGE} \
            dotnet restore "${SOLUTION}"
        '''
      }
    }

    stage('Build') {
      steps {
        sh '''
          docker run --rm \
            -v "$PWD":/src -w /src \
            ${DOTNET_SDK_IMAGE} \
            dotnet build "${SOLUTION}" -c Release --no-restore
        '''
      }
    }

    stage('Test') {
      steps {
        sh '''
          docker run --rm \
            -v "$PWD":/src -w /src \
            ${DOTNET_SDK_IMAGE} \
            dotnet test "${SOLUTION}" -c Release --no-build \
              --logger "trx;LogFileName=test_results.trx" \
              --results-directory "./TestResults"
        '''
      }
      post {
        always {
          archiveArtifacts artifacts: 'TestResults/**', allowEmptyArchive: true
        }
      }
    }

    stage('Docker Build') {
      steps {
        script {
          def shortCommit = sh(script: 'git rev-parse --short=8 HEAD', returnStdout: true).trim()
          def safeBranch  = (env.BRANCH_NAME ?: 'local').replaceAll('[^a-zA-Z0-9_.-]', '-')
          env.IMAGE_TAG   = "${safeBranch}-${env.BUILD_NUMBER}-${shortCommit}"
          env.FULL_IMAGE  = "${env.REGISTRY_HOST}/${env.IMAGE_NAME}:${env.IMAGE_TAG}"
        }

        sh '''
          docker build \
            -f "${DOCKERFILE_PATH}" \
            -t "${FULL_IMAGE}" \
            .
        '''
      }
    }

    stage('Registry Login & Push') {
      steps {
        withCredentials([usernamePassword(
          credentialsId: 'registry-creds',
          usernameVariable: 'REG_USER',
          passwordVariable: 'REG_PASS'
        )]) {
          sh '''
            echo "$REG_PASS" | docker login "${REGISTRY_HOST}" -u "$REG_USER" --password-stdin
            docker push "${FULL_IMAGE}"
          '''
        }

      }
    }
  }

  post {
    always {
      sh '''
        docker logout "${REGISTRY_HOST}" || true
      '''
    }
    cleanup {
      // Optional cleanup to keep disk under control on the agent
      sh '''
        docker image rm -f "${FULL_IMAGE}" || true
        if [ -n "${LATEST_IMAGE}" ]; then docker image rm -f "${LATEST_IMAGE}" || true; fi
      '''
    }
  }
}
