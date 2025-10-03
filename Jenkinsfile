pipeline {
    agent any

    environment {
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 'true'
        DOTNET_CLI_TELEMETRY_OPTOUT = 'true'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout([$class: 'GitSCM',
                    userRemoteConfigs: [[
                        url: 'https://github.com/TestingCodings/AutoGladiators.git',
                        credentialsId: 'github-token'
                    ]],
                    branches: [[name: '*/main'], [name: '*/next']]
                ])
            }
        }

        stage('Restore Dependencies') {
            steps {
                bat 'dotnet restore AutoGladiators.sln'
            }
        }

        stage('Build SharedLibs') {
            steps {
                bat 'dotnet build SharedLibs\\StateMachineLib\\StateMachineLib.csproj --configuration Release --no-restore'
                bat 'dotnet build SharedLibs\\BehaviorEngineLib\\BehaviorEngineLib.csproj --configuration Release --no-restore'
                bat 'dotnet build SharedLibs\\SkillTreeLib\\SkillTreeLib.csproj --configuration Release --no-restore'
            }
        }

        stage('Build Core') {
            steps {
                bat 'dotnet build AutoGladiators.Core\\AutoGladiators.Core.csproj --configuration Release --no-restore'
            }
        }

        stage('Build Tests') {
            steps {
                bat 'dotnet build AutoGladiators.Tests\\AutoGladiators.Tests.csproj --configuration Release --no-restore'
            }
        }

        stage('Run Unit Tests') {
            steps {
                script {
                    try {
                        bat 'dotnet test AutoGladiators.Tests\\AutoGladiators.Tests.csproj --configuration Release --no-build --logger "trx;LogFileName=test-results.trx" --logger "console;verbosity=normal"'
                    } catch (Exception e) {
                        echo "⚠️ Some tests failed, but continuing build process..."
                        currentBuild.result = 'UNSTABLE'
                    }
                }
            }
            post {
                always {
                    // Archive test result files
                    archiveArtifacts artifacts: '**/TestResults/*.trx', allowEmptyArchive: true
                    
                    // Try to publish test results if MSTest plugin is available
                    script {
                        try {
                            step([$class: 'MSTestPublisher', testResultsFile: '**/TestResults/*.trx'])
                        } catch (Exception e) {
                            echo "MSTest plugin not available, test results archived only"
                        }
                    }
                }
            }
        }

        stage('Build Game Client (Android)') {
            steps {
                bat 'dotnet build AutoGladiators.Client\\AutoGladiators_MAUI.csproj --configuration Release -f net8.0-android --no-restore'
            }
        }

        stage('Publish APK') {
            steps {
                bat 'dotnet publish AutoGladiators.Client\\AutoGladiators_MAUI.csproj --configuration Release -f net8.0-android -o publish\\ --no-build'
            }
        }

        stage('Archive APK') {
            steps {
                archiveArtifacts artifacts: 'publish\\**\\*.apk', allowEmptyArchive: true
                archiveArtifacts artifacts: '**/bin/Release/**/*.dll', allowEmptyArchive: true
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        failure {
            echo '❌ Build failed. Check console output above.'
        }
        success {
            echo '✅ Build and publish successful.'
        }
        unstable {
            echo '⚠️ Build completed with test failures or warnings.'
        }
    }
}
// This Jenkinsfile is designed to build and publish the AutoGladiators game client for Android using .NET MAUI.