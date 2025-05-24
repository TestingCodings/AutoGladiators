pipeline {
    agent any

    environment {
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 'true'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout([$class: 'GitSCM',
                    userRemoteConfigs: [[
                        url: 'https://github.com/TestingCodings/AutoGladiators.git',
                        credentialsId: 'github-token'
                    ]],
                    branches: [[name: '*/main']]
                ])
            }
        }

        stage('Build SharedLibs') {
            steps {
                bat 'dotnet build SharedLibs/ -c Release'
            }
        }

        stage('Build Client') {
            steps {
                bat 'dotnet build AutoGladiators.Client/ -c Release'
            }
        }

        stage('Test') {
            steps {
                bat 'dotnet test Tests/ --no-build --verbosity normal'
            }
        }

        stage('Publish Artifacts') {
            steps {
                bat 'dotnet publish AutoGladiators.Client/ -c Release -o publish/'
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'publish/**/*', allowEmptyArchive: true
        }
    }
}
