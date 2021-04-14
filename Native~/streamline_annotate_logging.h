/**
 * SPDX-License-Identifier: BSD-3-Clause
 *
 * Copyright (c) 2021, Arm Limited
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#ifndef STREAMLINE_ANNOTATE_LOGGING_H
#define STREAMLINE_ANNOTATE_LOGGING_H

//Mapped to android log levels - android_LogPriority
enum log_levels {
    LOG_UNKNOWN = 0,
    LOG_DEFAULT,
    LOG_VERBOSE,
    LOG_DEBUG,
    LOG_INFO,
    LOG_WARN,
    LOG_ERROR,
    LOG_FATAL,
    LOG_SILENT
};

/* ANDROID IMPLEMENTATION */
#if defined(ANDROID) || defined(__ANDROID__)
#include <android/log.h>

#define LOG_TAG "AnnotationLog"

#define LOGGING(LOG_LEVEL, fmt, ...)                                                                                   \
    __android_log_print(LOG_LEVEL, LOG_TAG, "%s/%s:%d " fmt, __FILE__, __FUNCTION__, __LINE__, ##__VA_ARGS__);

/* LINUX IMPLEMENTATION */
#elif defined(linux) || defined(__linux) || defined(__linux__)
// clang-format off
char *log_levels[] = { "UNKNOWN",
                       "DEFAULT",
                       "VERBOSE",
                       "DEBUG",
                       "INFO",
                       "WARN",
                       "ERROR",
                       "FATAL",
                       "SILENT"};
// clang-format on
#define LOGGING(LOG_LEVEL, fmt, ...)                                                                                   \
    printf("%s/%s:%d [%s] " fmt " \n", __FILE__, __func__, __LINE__, log_levels[LOG_LEVEL], ##__VA_ARGS__);

#endif
//Use to do logging, if not needed un-define this variable
#define ENABLE_LOG

#ifdef ENABLE_LOG
#define LOG(LOG_LEVEL, fmt, ...) LOGGING(LOG_LEVEL, fmt, ##__VA_ARGS__)
#else
#define LOG(LOG_LEVEL, fmt, ...) // nothing
#endif

#endif /* STREAMLINE_ANNOTATE_LOGGING_H */